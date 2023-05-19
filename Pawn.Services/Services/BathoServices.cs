using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.IDataAccess;
using Pawn.ViewModel.Models;
using Pawn.Libraries;
using Pawn.Core.DataModel;
using Pawn.Logger;
using System.Linq.Dynamic;
using System.IO;
using System.Data;

namespace Pawn.Services
{
    public class BathoServices : IBathoServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICustomerServices _customerServices;
        private readonly IHistoryServices _historyServices;
        private readonly IFileServices _file;
        private readonly ICashBookServices _cashBookServices;
        private readonly IDebtServices _debt;

        public BathoServices(IUnitOfWork unitOfWork, IMapper mapper, ICustomerServices customerServices, IHistoryServices historyServices,
                             IFileServices file, ICashBookServices cashBookServices, IDebtServices debt)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _customerServices = customerServices;
            _historyServices = historyServices;
            _file = file;
            _cashBookServices = cashBookServices;
            _debt = debt;
        }

        #region Code A T.A
        public MessageModels AddBHContract(BatHoModels bhModel)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thêm mới bát họ thất bại!"
            };

            try
            {
                var customerId = 0;
                var result = -1;
                var checkCode = _unitOfWork.BatHoRepository.Filter(s => s.Code.ToUpper() == bhModel.Code.ToUpper());
                if (checkCode.Any())
                {
                    return message = new MessageModels
                    {
                        Type = MessageTypeEnum.Error,
                        Message = $"Mã hợp đồng đã tồn tại!"
                    };
                }

                if (!bhModel.IsSystem)
                {
                    var customer = new Tb_Customer()
                    {
                        CreatedUser = RDAuthorize.Username,
                        Fullname = bhModel.CustomerName,
                        IdentityCard = bhModel.IdentityCard,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Phone = bhModel.Phone,
                        Address = bhModel.Address,
                        StoreId = bhModel.StoreId
                    };
                    _unitOfWork.CustomerRepository.Insert(customer);
                    result = _unitOfWork.SaveChanges();
                    customerId = customer.Id;
                    bhModel.CustomerId = customerId;
                }

                if (result > 0 || bhModel.IsSystem)
                {
                    var bhContract = _mapper.Map<BatHoModels, Tb_BHContract>(bhModel);
                    bhContract.MoneyPerOnce = (decimal)(bhModel.TotalMoney / bhModel.LoanTime) * bhModel.Frequency;
                    _unitOfWork.BatHoRepository.Insert(bhContract);
                    result = _unitOfWork.SaveChanges();
                    bhModel.Id = bhContract.Id;

                    if (result > 0)
                    {
                        HistoryModels historyClose = new HistoryModels
                        {
                            ContractID = bhModel.Id,
                            Content = "Tạo mới hợp đồng.",
                            StoreId = RDAuthorize.Store.Id,
                            TypeHistory = (int)DocumentTypeEnum.BatHo,
                            DebitMoney = (decimal)bhContract.MoneyForGues,
                            CreatedBy = RDAuthorize.Username,
                            CreatedDate = DateTime.Now
                        };
                        _historyServices.AddHistoryMessage(historyClose);

                        UpdateInfoInstallment((decimal)bhModel.TotalMoney, 0, (decimal)(bhModel.TotalMoney - bhModel.MoneyForGues), 0);

                        var cusname = _unitOfWork.CustomerRepository.Find(s => s.Id == bhContract.CustomerId)?.Fullname ?? "";
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            DebitAccount = (decimal)bhContract.MoneyForGues,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.BatHo,
                            VoucherType = 2,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            IsDeleted = false,
                            Customer = cusname,
                            Note = Utility.GetDescriptionC6(NotesEnum.GiaiNganHopDongBatHo),
                            NoteId = (int)NotesEnum.GiaiNganHopDongBatHo,
                            ContractId = historyClose.ContractID
                        });

                        if (bhModel.MoneyIntroduce.HasValue && bhModel.MoneyIntroduce > 0)
                            _cashBookServices.AddCashBook(new CashBookModals
                            {
                                CreditAccount = 0,
                                DebitAccount = bhModel.MoneyIntroduce ?? 0,
                                Note = "Tiền giới thiệu HD" + bhModel.Code,
                                NoteId = (int)NotesEnum.TienGioiThieuHopDongBatHo,
                                DocumentDate = DateTime.Now,
                                DocumentType = (int)DocumentTypeEnum.BatHo,
                                VoucherType = (int)VocherTypeEnum.PhieuChi,
                                StoreId = RDAuthorize.Store.Id,
                                IsActive = true,
                                IsDeleted = false,
                                Customer = cusname,
                                ContractId = historyClose.ContractID
                            });

                        if (bhModel.MoneyServices.HasValue && bhModel.MoneyServices > 0)
                            _cashBookServices.AddCashBook(new CashBookModals
                            {
                                CreditAccount = bhModel.MoneyServices ?? 0,
                                DebitAccount = 0,
                                Note = "Tiền dịch vụ HD" + bhModel.Code,
                                NoteId = (int)NotesEnum.TienDichVuHopDongBatHo,
                                DocumentDate = DateTime.Now,
                                DocumentType = (int)DocumentTypeEnum.BatHo,
                                VoucherType = (int)VocherTypeEnum.PhieuThu,
                                StoreId = RDAuthorize.Store.Id,
                                IsActive = true,
                                IsDeleted = false,
                                Customer = cusname,
                                ContractId = historyClose.ContractID
                            });
                        result = AddBatHoPay(bhModel);
                    }
                }
                if (result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    message.Message = "Thêm bát họ thành công!";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> AddBHContract ", ex);
            }

            return message;
        }

        public MessageModels UpdateBatHo(BatHoModels bhModel)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Chỉnh sửa bát họ thất bại!"
            };

            try
            {
                var customerId = 0;
                var result = -1;

                if (!bhModel.IsSystem)
                {
                    var customer = new Tb_Customer()
                    {
                        CreatedUser = RDAuthorize.Username,
                        Fullname = bhModel.CustomerName,
                        IdentityCard = bhModel.IdentityCard,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Phone = bhModel.Phone,
                        Address = bhModel.Address,
                        StoreId = bhModel.StoreId
                    };
                    _unitOfWork.CustomerRepository.Insert(customer);
                    result = _unitOfWork.SaveChanges();
                    customerId = customer.Id;
                    bhModel.CustomerId = customerId;
                }
                else
                {
                    var cus = _unitOfWork.CustomerRepository.Find(s => s.Id == bhModel.CustomerId);
                    if (cus != null)
                    {
                        cus.IdentityCard = bhModel.IdentityCard;
                        cus.Phone = bhModel.Phone;
                        cus.ICCreateDate = bhModel.ICCreateDate;
                        cus.ICCreatePlace = bhModel.ICCreatePlace;
                        cus.Address = bhModel.Address;
                    }
                }
                double money = 0;
                if (result > 0 || bhModel.IsSystem)
                {
                    var bh = _unitOfWork.BatHoRepository.Find(s => s.Id == bhModel.Id);
                    if (bh != null)
                    {
                        money = bh.MoneyForGues;
                        bh.TotalMoney = bhModel.TotalMoney;
                        bh.MoneyForGues = bhModel.MoneyForGues;
                        bh.LoanTime = bhModel.LoanTime;
                        bh.Frequency = bhModel.Frequency;
                        bh.FromDate = bhModel.FromDate;
                        bh.Note = bhModel.Note;
                        bh.IsBefore = bhModel.IsBefore;
                        bh.StaffManagerId = bhModel.StaffManagerId;
                        bh.CustomerId = bhModel.CustomerId;
                        bh.UpdatedDate = DateTime.Now;
                        bh.UpdatedUser = RDAuthorize.Username;
                        bh.MoneyPerOnce = (decimal)(bhModel.TotalMoney / bhModel.LoanTime) * bhModel.Frequency;
                    }
                    result = _unitOfWork.SaveChanges();
                    bhModel.Id = bh.Id;

                    if (result > 0)
                    {
                        HistoryModels historyClose = new HistoryModels
                        {
                            ContractID = bhModel.Id,
                            Content = "Chỉnh sửa hợp đồng.",
                            StoreId = RDAuthorize.Store.Id,
                            TypeHistory = (int)DocumentTypeEnum.BatHo,
                            DebitMoney = (decimal)bhModel.MoneyForGues,
                            HavingMoney = (decimal)money,
                            CreatedBy = RDAuthorize.Username,
                            CreatedDate = DateTime.Now
                        };
                        _historyServices.AddHistoryMessage(historyClose);

                        UpdateInfoInstallment((decimal)bhModel.TotalMoney, 0, (decimal)(bhModel.TotalMoney - bhModel.MoneyForGues), 0);

                        var cusname = _unitOfWork.CustomerRepository.Find(s => s.Id == bh.CustomerId)?.Fullname ?? "";
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            CreditAccount = (decimal)historyClose.HavingMoney,
                            DebitAccount = (decimal)historyClose.DebitMoney,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.BatHo,
                            VoucherType = 2,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            IsDeleted = false,
                            Customer = cusname,
                            Note = historyClose.Content,
                            ContractId = historyClose.ContractID
                        });
                        result = AddBatHoPay(bhModel, true);
                    }
                }
                if (result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    message.Message = "Chỉnh sửa họ thành công!";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> UpdateBatHo ", ex);
            }

            return message;
        }

        public string GetMaxBHContract()
        {
            string maxvalue = "1";
            var code = _unitOfWork.BatHoRepository.OrderByDescending(s => s.Id).FirstOrDefault();
            if (code != null)
                maxvalue = RDAuthorize.Store.Id + "" + ((code?.Id ?? 0) + 1);
            return maxvalue;
        }

        public List<BatHoModels> LoadDataBatHo(string strContractId, string strCustomerName, DateTime? dtFromDate, DateTime? dtToDate, 
            int? intTimePay, int? intStatusID, int intPageSize, int intPageIndex, string sortcolumn, string sorttype, int staffId)
        {
            try
            {
                var _params = new object[] { RDAuthorize.Store.Id, strContractId, staffId, strCustomerName, dtToDate, dtFromDate, intTimePay, intStatusID, intPageSize, intPageIndex - 1, sortcolumn + " " + sorttype };
                var data = _unitOfWork.ExecStoreProdure<BatHoModels>("SP_BATHO_SRH {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}", _params).ToList();
                data = data.Skip((intPageIndex - 1) * intPageSize).Take(intPageSize).ToList();
                foreach (var item in data)
                {
                    item.StatusContract = StatusContractEnum.TatCaHDDangvay;
                    if (item.IsDeleted) item.StatusContract = StatusContractEnum.DaXoa;
                    else if (item.IsCloseContract) item.StatusContract = StatusContractEnum.KetThuc;
                    else if (item.IsBadDebt) item.StatusContract = StatusContractEnum.NoXau;
                    else
                    {
                        var todate = item.DocumentDate.AddDays(item.LoanTime - 1);
                        if (DateTime.Now.Date > todate.Date) item.StatusContract = StatusContractEnum.QuaHan;
                        else
                        {
                            var pay = _unitOfWork.BatHoPayRepository.Filter(s => s.BathoId == item.Id && s.IsPaid == false).OrderBy(s => s.FromDate)
                                                 .FirstOrDefault();
                            if (DateTime.Now.AddDays(1).Date == todate.Date) item.StatusContract = StatusContractEnum.NgayMaiDongHo; // ngay cuoi dong ho
                            if (pay != null)
                            {
                                if (item.IsBefore)
                                {
                                    if ((DateTime.Now.Date - pay.FromDate.Date).TotalDays >= 1) item.StatusContract = StatusContractEnum.ChamHo;
                                }
                                else
                                {
                                    if ((DateTime.Now.Date - pay.ToDate.Date).TotalDays >= 1) item.StatusContract = StatusContractEnum.ChamHo;
                                }
                            }
                        }
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> LoadDataBatHo ", ex);
                return new List<BatHoModels>();
            }
        }

        #endregion

        #region Code A Nam
        public int GetPayMaxBH(int contractId)
        {
            int maxvalue = _unitOfWork.BatHoPayRepository.Filter(s => s.BathoId == contractId)
                .Max(s => s.Id);
            return maxvalue;
        }

        public MessageModels UpdateBatHo(int id)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thay đổi thất bại!"
            };
            try
            {
                var result = -1;
                var model = _unitOfWork.BatHoRepository.Filter(x => x.Id == id).FirstOrDefault();
                // đóng hợp đồng của bát họ cũ
                if (model != null)
                {
                    model.ClosedDate = DateTime.Now;
                    model.ClosedUser = RDAuthorize.Username;
                    model.IsCloseContract = true;
                    model.UpdatedDate = DateTime.Now;
                    model.UpdatedUser = RDAuthorize.Username;

                    result = _unitOfWork.SaveChanges();
                }
                if (result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    message.Message = $"Thay đổi thành công!";
                }

            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> UpdateBatHoPay ", ex);
            }
            return message;
        }

        public BatHoModels getBHById(int id)
        {
            var data = _unitOfWork.BatHoRepository.Find(s => s.Id == id);
            var result = _mapper.Map<Tb_BHContract, BatHoModels>(data);
            return result;
        }

        public MessageModels CloseContract(int originalId, decimal moneyNumberd)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Đóng hợp đồng thất bại!"
            };
            try
            {
                if (moneyNumberd != 0)
                {
                    _debt.AddDebt(new DebtModels
                    {
                        ContractId = originalId,
                        IsDebt = moneyNumberd > 0, // nếu là tiền thừa thì sẽ ghi nợ và ngược lại
                        MoneyNumber = Math.Abs(moneyNumberd),
                        DocumentType = (int)DocumentTypeEnum.BatHo,
                    });
                    message = CloseContract(originalId, (double)moneyNumberd);
                }
                else message = CloseContract(originalId);
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> CloseContract 3 params", ex);
            }
            return message;
        }

        public MessageModels CloseContract(int originalId, double moneyOther = 0)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Đóng hợp đồng thất bại!"
            };
            try
            {
                var payIdMax = GetPayMaxBH(originalId);
                var payModel = new BatHoPayModels
                {
                    Id = payIdMax,
                    BathoId = originalId,
                    IsPaid = true
                };

                // Tạo mới từng phiếu thanh toán còn lại ở bát họ cũ
                message = BatHoPay(payModel, true, (decimal)moneyOther);
                if (message.Type == MessageTypeEnum.Success)
                {
                    message = UpdateBatHo(originalId);
                    HistoryModels historyClose = new HistoryModels
                    {
                        ContractID = originalId,
                        ActionDate = DateTime.Now,
                        Content = "Đóng hợp đồng.",
                        StoreId = RDAuthorize.Store.Id,
                        CreatedDate = DateTime.Now,
                        CreatedBy = RDAuthorize.Username,
                        TypeHistory = (int)DocumentTypeEnum.BatHo,
                    };
                    message = _historyServices.AddHistoryMessage(historyClose);
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> CloseContract ", ex);
            }
            return message;
        }
        #endregion

        #region Code Tuan
        public BatHoModels LoadDetailBatHoModel(int id)
        {
            var batho = _unitOfWork.BatHoRepository.Filter(s => s.Id == id && s.StoreId == RDAuthorize.Store.Id && s.IsDeleted == false)
                                                     .Join(_unitOfWork.CustomerRepository.Filter(s => s.StoreId == RDAuthorize.Store.Id),
                                                           bh => bh.CustomerId,
                                                           cs => cs.Id,
                                                           (bh, cs) => new { bh, cs }).
                                                      Select(s => new BatHoModels
                                                      {
                                                          ClosedDate = s.bh.ClosedDate,
                                                          LoanTime = s.bh.LoanTime,
                                                          Frequency = s.bh.Frequency ?? 0,
                                                          ClosedUser = s.bh.ClosedUser,
                                                          CustomerId = s.bh.CustomerId,
                                                          FromDate = s.bh.FromDate,
                                                          Id = s.bh.Id,
                                                          IsCloseContract = s.bh.IsCloseContract,
                                                          MoneyForGues = s.bh.MoneyForGues,
                                                          TotalMoney = s.bh.TotalMoney,
                                                          Note = s.bh.Note,
                                                          CustomerName = s.cs.Fullname,
                                                          Address = s.cs.Address,
                                                          IdentityCard = s.cs.IdentityCard,
                                                          ICCreateDate = s.cs.ICCreateDate,
                                                          ICCreatePlace = s.cs.ICCreatePlace,
                                                          Phone = s.cs.Phone,
                                                          IsSystem = true,
                                                          Code = s.bh.Code,
                                                          IsBefore = s.bh.IsBefore,
                                                          IsBadDebt = s.bh.IsBadDebt,
                                                          FromDateDetail = s.bh.FromDate,
                                                          MoneyIntroduce = s.bh.MoneyIntroduce,
                                                          MoneyServices = s.bh.MoneyServices,
                                                          StaffManagerId = s.bh.StaffManagerId,
                                                          StoreId = s.bh.StoreId,

                                                      }).FirstOrDefault() ?? new BatHoModels();

            batho.FromDateString = batho.FromDateDetail.ToString(Constants.DateFormat);
            batho.IsPaid = _unitOfWork.BatHoPayRepository.Filter(s => s.IsPaid == true && s.BathoId == batho.Id).Any();
            return batho;
        }
        public BatHoModels LoadDetailBatHo(int id)
        {
            try
            {
                var batho = _unitOfWork.BatHoRepository.Filter(s => s.Id == id && s.StoreId == RDAuthorize.Store.Id && s.IsDeleted == false)
                                                       .Join(_unitOfWork.CustomerRepository.Filter(s => s.StoreId == RDAuthorize.Store.Id),
                                                             bh => bh.CustomerId,
                                                             cs => cs.Id,
                                                             (bh, cs) => new { bh, cs.Fullname }).
                                                        Select(s => new BatHoModels
                                                        {
                                                            ClosedDate = s.bh.ClosedDate,
                                                            LoanTime = s.bh.LoanTime,
                                                            Frequency = s.bh.Frequency ?? 0,
                                                            ClosedUser = s.bh.ClosedUser,
                                                            CustomerId = s.bh.CustomerId,
                                                            FromDate = s.bh.FromDate,
                                                            Id = s.bh.Id,
                                                            IsCloseContract = s.bh.IsCloseContract,
                                                            MoneyForGues = s.bh.MoneyForGues,
                                                            TotalMoney = s.bh.TotalMoney,
                                                            Note = s.bh.Note,
                                                            CustomerName = s.Fullname,
                                                            Code = s.bh.Code,
                                                            DocumentName = s.bh.DocumentName,
                                                            FromDateDetail = s.bh.FromDate
                                                            //MoneyPerOnce = s.bh.MoneyPerOnce
                                                        }).FirstOrDefault() ?? new BatHoModels();
                batho.ToDateString = batho.FromDate.AddDays(batho.LoanTime - 1).ToString("dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                //var lst = LoadHistoryBHPay(id);
                int doctype = (int)DocumentTypeEnum.BatHo;
                batho.ListBatHoPay = LoadHistoryBHPay(id);
                var paymentRequired = batho.ListBatHoPay.Where(s => s.IsPaid).ToList().Sum(s => s.PaymentNeedMoney) ?? 0;
                var moneyCustomer = batho.ListBatHoPay.Where(s => s.IsPaid).ToList().Sum(s => s.MoneyOfCustomer) ?? 0;
                var debtInPay = moneyCustomer - paymentRequired;

                var lstDebt = _unitOfWork.DebtRepository.Filter(s => s.ContractId == id && s.DocumentType == doctype).ToList();
                var debt = lstDebt.Where(s => s.IsDebt).Sum(s => s.MoneyNumber);
                var hav = lstDebt.Where(s => s.IsDebt == false).Sum(s => s.MoneyNumber);
                batho.TotalHaving = hav - debt;
                batho.MoneyOrther = hav - debt + debtInPay;
                return batho;
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> LoadDetailBatHo ", ex);
                return new BatHoModels();
            }
        }

        public List<BatHoPayModels> LoadHistoryBHPay(int id)
        {
            try
            {
                var lstData = _unitOfWork.BatHoPayRepository.Filter(s => s.BathoId == id).OrderBy(s => s.FromDate).ToList();
                var lstDataModel = _mapper.Map<List<Tb_BH_Pay>, List<BatHoPayModels>>(lstData);
                for (int i = 0; i < lstDataModel.Count; i++)
                    lstDataModel[i].NumberOfDays = (lstDataModel[i].ToDate - lstDataModel[i].FromDate).TotalDays + 1;
                var item = lstDataModel.FirstOrDefault(s => s.IsPaid == false);
                if (item != null)
                {
                    var index = lstDataModel.FindIndex(s => s == item);
                    lstDataModel[index].LoanDate = DateTime.Now;
                    lstDataModel[index].MoneyOfCustomer = item.PaymentNeedMoney;
                    lstDataModel[index].IsCurrent = true;
                }
                return lstDataModel;
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> LoadHistoryBHPay ", ex);
                return new List<BatHoPayModels>();
            }
        }

        public MessageModels BatHoPay(BatHoPayModels items, bool isClose = false, decimal moneyOrther = 0)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"{(items.IsPaid ? "Thanh" : "Hủy")} toán từ ngày {items.FromDate:dd-MM-yyyy} đến ngày {items.ToDate:dd-MM-yyyy} thất bại!"
            };

            try
            {
                var result = -1;
                decimal total = 0;
                //decimal debt = 0;
                if (items.IsPaid == false)
                {
                    var obj = _unitOfWork.BatHoPayRepository.Find(s => s.Id == items.Id);
                    total = obj.MoneyOfCustomer ?? 0;
                    if (obj != null)
                    {
                        obj.IsPaid = items.IsPaid;
                        obj.MoneyOfCustomer = null;
                        obj.LoanDate = null;
                    }
                    result = _unitOfWork.SaveChanges();
                    UpdateBatHoPay2(items.BathoId);
                }
                else
                {
                    var bhPay = _unitOfWork.BatHoPayRepository.Filter(s => s.Id <= items.Id && s.BathoId == items.BathoId && s.IsPaid == false).ToList();
                    foreach (var item in bhPay)
                    {
                        var obj = _unitOfWork.BatHoPayRepository.Find(s => s.Id == item.Id);
                        //if (bhPay.Count() == 1)
                        //{
                        //    debt = items.MoneyOfCustomer ?? 0 - obj.PaymentNeedMoney ?? 0;
                        //}
                        if (obj != null)
                        {
                            if (obj.IsPaid == true)
                            {
                                message.Type = MessageTypeEnum.Warning;
                                message.Message = "Kì họ này đã có nhân viên cập nhật. Xin vui lòng làm mới lại trang";
                                return message;
                            }

                            obj.IsPaid = items.IsPaid;
                            obj.MoneyOfCustomer = (bhPay.Count() > 1 || isClose == true) ? obj.PaymentNeedMoney : items.MoneyOfCustomer;
                            obj.LoanDate = (bhPay.Count() > 1 || isClose == true) ? DateTime.Now : items.LoanDate;
                            total += obj.MoneyOfCustomer ?? 0;
                        }
                    }
                    result = _unitOfWork.SaveChanges();
                }
                if (result > 0)
                {
                    var history = new HistoryModels
                    {
                        Content = items.IsPaid ? "Đóng tiền họ" : "Hủy đóng tiền họ",
                        TypeHistory = (int)DocumentTypeEnum.BatHo,
                        ContractID = items.BathoId,
                        StoreId = RDAuthorize.Store.Id
                    };
                    if (items.IsPaid) history.HavingMoney = total - moneyOrther;
                    else history.DebitMoney = total;
                    _historyServices.AddHistory(history);
                    var cusid = _unitOfWork.BatHoRepository.Find(s => s.Id == items.BathoId)?.CustomerId ?? -1;
                    var cusname = _unitOfWork.CustomerRepository.Find(s => s.Id == cusid)?.Fullname ?? "";
                    _cashBookServices.AddCashBook(new CashBookModals
                    {
                        DebitAccount = history.DebitMoney ?? 0,
                        CreditAccount = history.HavingMoney ?? 0,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.BatHo,
                        VoucherType = items.IsPaid ? 1 : 2,
                        StoreId = RDAuthorize.Store.Id,
                        IsActive = true,
                        IsDeleted = false,
                        Customer = cusname,
                        Note = history.Content,
                        NoteId = items.IsPaid ? (int)NotesEnum.DongTienHo : (int)NotesEnum.HuyDongTienHo,
                        ContractId = history.ContractID
                    });
                }

                message.Type = result > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = $"{(items.IsPaid ? "Thanh" : "Hủy")} toán từ ngày {items.FromDate:dd-MM-yyyy} đến ngày {items.ToDate:dd-MM-yyyy} {(result > 0 ? "thành công" : "thất bại")}!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> BatHoPay ", ex);
            }

            return message;

        }

        public int AddBatHoPay(BatHoModels item, bool isUpdate = false)
        {
            var result = -1;
            DateTime? fromdate = null;
            decimal totalPaid = 0;
            try
            {
                if (isUpdate)
                {

                    _unitOfWork.BatHoPayRepository.Delete(s => s.BathoId == item.Id && s.IsPaid == false);
                    _unitOfWork.SaveChanges();
                    var dataPaid = _unitOfWork.BatHoPayRepository.Filter(s => s.BathoId == item.Id && item.IsPaid == true).OrderByDescending(s => s.FromDate).ToList();
                    var getFromDate = dataPaid.FirstOrDefault();
                    totalPaid = dataPaid.Sum(s => s.PaymentNeedMoney) ?? 0;
                    if (getFromDate != null)
                        fromdate = getFromDate.ToDate.AddDays(1);
                }
                var listBatHoPay = new List<BatHoPayModels>();
                var endDate = item.FromDate.AddDays(item.LoanTime - 1);
                var fromDate = fromdate ?? item.FromDate;
                while (true)
                {
                    var todate = fromDate.AddDays(item.Frequency - 1);
                    if (fromDate >= endDate || todate >= endDate)
                    {
                        if (listBatHoPay.Any(x => x.ToDate == endDate)) break;
                        var money = (decimal)item.TotalMoney - totalPaid - (listBatHoPay.Sum(s => s.PaymentNeedMoney) ?? 0);
                        listBatHoPay.Add(new BatHoPayModels
                        {
                            FromDate = fromDate,
                            ToDate = endDate,
                            LoanDate = null,
                            MoneyOfCustomer = null,
                            IsPaid = false,
                            PaymentNeedMoney = money,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            BathoId = item.Id,
                            NumberOfDays = (endDate - fromDate).TotalDays
                        });
                        break;
                    }
                    listBatHoPay.Add(new BatHoPayModels
                    {
                        FromDate = fromDate,
                        ToDate = todate,
                        LoanDate = null,
                        MoneyOfCustomer = null,
                        IsPaid = false,
                        PaymentNeedMoney = (decimal)(item.TotalMoney / item.LoanTime) * item.Frequency,
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        BathoId = item.Id,
                        NumberOfDays = (endDate - fromDate).TotalDays
                    });
                    fromDate = fromDate.AddDays(item.Frequency);
                }
                var lstBhPayDay = _mapper.Map<List<BatHoPayModels>, List<Tb_BH_Pay>>(listBatHoPay);
                _unitOfWork.BatHoPayRepository.InsertRange(lstBhPayDay);
                result = _unitOfWork.SaveChanges();

            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> AddBatHoPay ", ex);
            }
            return result;
        }

        public MessageModels UpdateBatHoPay(decimal money, int idBatho)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thay đổi thất bại!"
            };
            try
            {
                var obj = _unitOfWork.BatHoRepository.Find(s => s.Id == idBatho);
                if (obj != null)
                {
                    obj.MoneyPerOnce = money;
                    _unitOfWork.SaveChanges();
                    message = UpdateBatHoPay2(idBatho);
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> UpdateBatHoPay ", ex);
            }
            return message;
        }

        private MessageModels UpdateBatHoPay2(int idBatho)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thay đổi thất bại!"
            };
            try
            {
                var objBatho = _unitOfWork.BatHoRepository.Find(s => s.Id == idBatho);
                var result = -1;
                var lstData = _unitOfWork.BatHoPayRepository.Filter(s => s.BathoId == idBatho).OrderBy(s => s.FromDate).ToList();
                var lstChange = lstData.Where(s => s.IsPaid == false).ToList();
                if (lstChange.Any() && lstChange[0].PaymentNeedMoney != objBatho.MoneyPerOnce)
                {
                    var total = lstData.Where(s => s.IsPaid == true).ToList().Sum(s => s.PaymentNeedMoney) ?? 0;
                    for (int i = 0; i < lstChange.Count; i++)
                    {
                        var id = lstChange[i].Id;
                        var obj = _unitOfWork.BatHoPayRepository.Find(s => s.Id == id);
                        if (obj != null)
                        {
                            obj.PaymentNeedMoney = (i == (lstChange.Count - 1)) ? ((decimal)objBatho.TotalMoney - total) : (objBatho.MoneyPerOnce ?? 0);
                            total += (objBatho.MoneyPerOnce ?? 0);
                        }
                    }
                    result = _unitOfWork.SaveChanges();
                }
                else result = 1;

                message.Type = result > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = $"Thay đổi {(result > 0 ? "thành công" : "thất bại")}!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> UpdateBatHoPay ", ex);
            }
            return message;
        }

        public Tb_InfoInstallment AddInfo()
        {
            Tb_InfoInstallment info;
            try
            {
                var docType = (int)DocumentTypeEnum.BatHo;
                info = _unitOfWork.InfoInstallmentRepository.Find(s => s.DocumentType == docType);
                if (info == null)
                {
                    info = new Tb_InfoInstallment
                    {
                        DocumentType = docType,
                        StoreId = RDAuthorize.Store.Id,
                        TotalInterestEarned = 0,
                        TotalInterestExpected = 0,
                        TotalMoneyDebtor = 0,
                        TotalMoneyInvestment = 0
                    };
                    _unitOfWork.InfoInstallmentRepository.Insert(info);
                    _unitOfWork.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                info = new Tb_InfoInstallment();
                PawnLog.Error("BathoServices --> AddInfo ", ex);
            }
            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="totalInterestEarned">TIỀN CHO VAY</param>
        /// <param name="totalInterestExpected">TIỀN NỢ</param>
        /// <param name="totalMoneyDebtor">LÃI DỰ KIẾN</param>
        /// <param name="totalMoneyInvestment">LÃI ĐÃ THU</param>
        public void UpdateInfoInstallment(decimal totalInterestEarned, decimal totalInterestExpected, decimal totalMoneyDebtor, decimal totalMoneyInvestment)
        {
            try
            {
                var info = AddInfo();
                info.TotalInterestEarned += totalInterestEarned;
                info.TotalInterestExpected += totalInterestExpected;
                info.TotalMoneyDebtor += totalMoneyDebtor;
                info.TotalMoneyInvestment += totalMoneyInvestment;

                _unitOfWork.InfoInstallmentRepository.Update(info);
                _unitOfWork.SaveChanges();
            }
            catch (Exception)
            {

                throw;
            }
        }
        //public List<BatHoPayModels> LoadDateBhPay(BatHoModels item, List<BatHoPayModels> listDate)
        //{
        //    var listBatHoPay = listDate;
        //    var endDate = item.FromDate.AddDays(item.LoanTime - 1);
        //    var date = listDate.FirstOrDefault(s => s.IsPaid == false && s.BathoId == item.Id).ToDate.AddDays(1);
        //    listDate.FirstOrDefault(s => s.IsPaid == false && s.BathoId == item.Id).LoanDate = DateTime.Now;
        //    while (true)
        //    {
        //        if(date >= endDate)
        //        {
        //            if (listBatHoPay.Any(x => x.ToDate == endDate)) break;
        //                listBatHoPay.Add(new BatHoPayModels
        //                {
        //                    FromDate = date,
        //                    ToDate = endDate,
        //                    MoneyPerOnce = (decimal)item.TotalMoney - (listBatHoPay.Sum(s => s.MoneyPerOnce) ?? 0),
        //                    LoanDate = DateTime.Now,
        //                    MoneyOfCustomer = item.MoneyPerOnce ?? 0,
        //                    IsPaid = false
        //                });
        //            break;
        //        }
        //        listBatHoPay.Add(new BatHoPayModels
        //        {
        //            FromDate = date,
        //            ToDate = date.AddDays(item.Frequency - 1),
        //            MoneyPerOnce = item.MoneyPerOnce,
        //            LoanDate = null,
        //            MoneyOfCustomer = 0,
        //            IsPaid = false
        //        });
        //        date = date.AddDays(item.Frequency);
        //    }
        //    return listBatHoPay;
        //}

        public List<BatHoModels> LoadDataCustomerPaidTomorrow(InterestPaid addDay)
        {
            try
            {
                var _params = new object[] { RDAuthorize.Store.Id, (int)addDay };
                var data = _unitOfWork.ExecStoreProdure<BatHoModels>("SP_LOAD_CUSTOMER_PAID_TOMORROW_BH {0}, {1}", _params).ToList();
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> LoadDataCustomerPaidTomorrow ", ex);
                return new List<BatHoModels>();
            }
        }

        public MessageModels UpdateBadDebt(int idBatHo, bool isBadDebt)
        {
            var mess = new MessageModels
            {
                Message = "Đã chuyển hợp đồng thành nợ xấu thất bại",
                Type = MessageTypeEnum.Error
            };
            try
            {
                var rs = -1;
                var bh = _unitOfWork.BatHoRepository.Find(s => s.Id == idBatHo);
                if (bh != null)
                {
                    bh.IsBadDebt = isBadDebt;
                    bh.UpdatedDate = DateTime.Now;
                    bh.UpdatedUser = RDAuthorize.Username;
                    rs = _unitOfWork.SaveChanges();
                }
                if (rs > 0)
                {
                    mess.Type = MessageTypeEnum.Success;
                    mess.Message = "Đã chuyển hợp đồng thành nợ xấu";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> UpdateBadDebt ", ex);
            }
            return mess;
        }

        public MessageModels DeleteBatHo(int idBatHo)
        {
            var message = new MessageModels
            {
                Message = "Xóa hợp đồng bát họ thất bại",
                Type = MessageTypeEnum.Error
            };

            try
            {
                var result = -1;
                var batho = _unitOfWork.BatHoRepository.Find(s => s.Id == idBatHo);
                if (batho != null)
                {
                    batho.IsDeleted = !batho.IsDeleted;
                    batho.DeletedDate = DateTime.Now;
                    batho.DeletedUser = RDAuthorize.Username;
                    result = _unitOfWork.SaveChanges();
                }
                if (result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    _cashBookServices.AddCashBook(new CashBookModals
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CreditAccount = batho.IsDeleted == true ? (decimal)batho.TotalMoney : 0,
                        DebitAccount = batho.IsDeleted == false ? (decimal)batho.TotalMoney : 0,
                        Note = "Xóa hợp đồng bát họ",
                        NoteId = (int)NotesEnum.XoaHDBatHo,
                        Customer = _unitOfWork.CustomerRepository.Find(s => s.Id == batho.CustomerId)?.Fullname,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.BatHo,
                        VoucherType = batho.IsDeleted ? (int)VocherTypeEnum.PhieuThu : (int)VocherTypeEnum.PhieuChi,
                        StoreId = RDAuthorize.Store.Id,
                        IsActive = true,
                        IsDeleted = false,
                        ContractId = idBatHo
                    });
                }
                message.Message = $"{(batho.IsDeleted ? "Xóa" : "Khôi phục")} hợp đồng bát họ thành công";
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> DeleteBatHo ", ex);
            }

            return message;
        }

        public MemoryStream ExportExcel()
        {
            MemoryStream stream;
            var excelPackage = new ExcelPackages("Danh sách hợp đồng bát họ");
            try
            {
                var _params = new object[] { RDAuthorize.Store.Id };
                var data = _unitOfWork.ExecStoreProdure<BatHoModels>("SP_BH_EXPORT_EXCEL {0}", _params).ToList();
                var dt = new DataTable();
                for (int i = 0; i < 14; i++)
                    dt.Columns.Add(i + "", typeof(string));
                dt.Rows.Add("Mã HĐ", "Tên KH", "SĐT", "Bát Họ", "Tiền giao khách", "Tỷ lệ", "Ngày bốc", "Ngày hết hạn"
                    , "Đã đóng đến ngày", "Đã đóng được", "Tiền còn phải đóng", "Ngày đóng họ tiếp theo", "Tiền thu theo kỳ", "Tiền nợ");
                foreach (var item in data)
                {
                    dt.Rows.Add(item.Code, item.CustomerName, item.Phone, item.TotalMoney.ToPrice(), item.MoneyForGues.ToPrice(), item.Rate, item.FromDateString,
                        item.ToDateString, item.ToDatePaid, item.TotalPay.ToPrice(), item.MoneyOrther.ToPrice(), item.PayDateString, item.MoneyPerOnce.ToPrice(), item.DebtMoney.ToPrice());
                }
                stream = excelPackage.ExportToExcel(dt);
            }
            catch (Exception ex)
            {
                stream = null;
            }
            return stream;
        }


        public MessageModels ConvertStore(int bathoId, int storeId)
        {
            var rs = -1;
            var cusid = -1;
            int docType = (int)DocumentTypeEnum.BatHo;
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Chuyển cửa hàng thất bại"
            };

            try
            {
                var bh = _unitOfWork.BatHoRepository.Find(s => s.Id == bathoId);
                if (bh != null)
                {
                    //if (bh.CreatedDate.Date < Convert.ToDateTime("10/29/2018").Date)
                    //{
                    //    message.Message = "Chỉ được chuyển những hợp đồng được tạo sau ngày 28-10-2018";
                    //    return message;
                    //}
                    var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == bh.CustomerId);
                    customer.StoreId = storeId;
                    _unitOfWork.CustomerRepository.Insert(customer);
                    rs = _unitOfWork.SaveChanges();
                    cusid = customer.Id;
                }

                if (rs > 0)
                {

                    bh.StoreId = storeId;
                    bh.CustomerId = cusid;

                    var cb = _unitOfWork.CashBookRepository.Filter(s => s.ContractId == bathoId && s.DocumentType == docType);
                    if (cb != null)
                    {
                        foreach (var item in cb)
                        {
                            item.StoreId = storeId;
                            _unitOfWork.CashBookRepository.Update(item);
                        }
                    }

                    var history = _unitOfWork.HistoryRepository.Filter(s => s.ContractID == bathoId && s.TypeHistory == docType);
                    if (history != null)
                    {
                        foreach (var item in history)
                        {
                            item.StoreId = storeId;
                            _unitOfWork.HistoryRepository.Update(item);
                        }
                    }

                    rs = _unitOfWork.SaveChanges();


                    if (rs > 0)
                    {
                        message.Type = MessageTypeEnum.Success;
                        message.Message = "Chuyển hợp đồng sang cửa hàng mới thành công";
                    }
                }

            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> DeleteBatHo ", ex);
            }

            return message;
        }


        public MessageModels UpdateMoneySI(int contractId, decimal? moneyIntroduce, decimal? moneyService)
        {
            var mess = new MessageModels
            {
                Message = "Cập nhật tiền dịch vụ và tiền giới thiệu thất bại",
                Type = MessageTypeEnum.Error
            };

            try
            {
                var rs = -1;
                var bh = _unitOfWork.BatHoRepository.Find(s => s.Id == contractId);
                decimal moneyIntro = 0;
                decimal moneySer = 0;
                if (bh != null)
                {
                    moneyIntro = bh.MoneyIntroduce ?? 0;
                    moneySer = bh.MoneyServices ?? 0;
                    bh.MoneyIntroduce = moneyIntroduce ?? 0;
                    bh.MoneyServices = moneyService ?? 0;
                    bh.UpdatedDate = DateTime.Now;
                    bh.UpdatedUser = RDAuthorize.Username;
                    rs = _unitOfWork.SaveChanges();
                }
                if (rs > 0)
                {
                    mess.Type = MessageTypeEnum.Success;
                    mess.Message = "Cập nhật tiền dịch vụ và tiền giới thiệu thành công";
                    var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == bh.CustomerId)?.Fullname;
                    if (moneyIntroduce.HasValue && moneyIntroduce > 0)
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            CreditAccount = moneyIntro,
                            DebitAccount = bh.MoneyIntroduce ?? 0,
                            Note = "Cập nhật tiền giới thiệu HD " + bh.Code,
                            NoteId = (int)NotesEnum.TienGioiThieuHopDongBatHo,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.BatHo,
                            VoucherType = (int)VocherTypeEnum.PhieuChi,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            Customer = customer,
                            IsDeleted = false,
                            ContractId = bh.Id
                        });

                    if (moneyService.HasValue && moneyService > 0)
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            CreditAccount = bh.MoneyServices ?? 0,
                            DebitAccount = moneySer,
                            Customer = customer,
                            Note = "Cập nhật tiền dịch vụ HD " + bh.Code,
                            NoteId = (int)NotesEnum.TienDichVuHopDongBatHo,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.BatHo,
                            VoucherType = (int)VocherTypeEnum.PhieuThu,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            IsDeleted = false,
                            ContractId = bh.Id
                        });
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> UpdateMoneySI ", ex);
            }

            return mess;
        }
        #endregion
    }
}
