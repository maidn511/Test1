using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.IDataAccess;
using Pawn.ViewModel.Models;
using Pawn.Core.DataModel;
using Pawn.Logger;
using Pawn.Libraries;
using System.IO;
using System.Data;
using System.Linq.Dynamic;

namespace Pawn.Services
{
    public class PawnServices : IPawnServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICustomerServices _customerServices;
        private readonly IHistoryServices _historyServices;
        private readonly IFileServices _file;
        private readonly ICashBookServices _cashBookServices;
        private readonly IDebtServices _debt;
        private readonly IExtentionContractServices _extentionContractServices;

        public PawnServices(IUnitOfWork unitOfWork, IMapper mapper, ICustomerServices customerServices, IHistoryServices historyServices,
                             IFileServices file, ICashBookServices cashBookServices, IDebtServices debt, IExtentionContractServices extentionContractServices)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _customerServices = customerServices;
            _historyServices = historyServices;
            _file = file;
            _cashBookServices = cashBookServices;
            _debt = debt;
            _extentionContractServices = extentionContractServices;
        }

        #region Code A T.A
        public MessageModels AddPawnContract(PawnContractModels vlModel)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thêm mới hợp đồng vay lãi thất bại!"
            };
            try
            {
                var customerId = 0;
                var result = -1;
                if (!vlModel.IsSystem)
                {
                    var customer = new Tb_Customer()
                    {
                        CreatedUser = RDAuthorize.Username,
                        Fullname = vlModel.CustomerName,
                        IdentityCard = vlModel.IdentityCard,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Phone = vlModel.Phone,
                        Address = vlModel.Address,
                        StoreId = vlModel.StoreId,
                        ICCreatePlace = vlModel.ICCreatePlace,
                        ICCreateDate = vlModel.ICCreateDate
                    };
                    _unitOfWork.CustomerRepository.Insert(customer);
                    result = _unitOfWork.SaveChanges();
                    customerId = customer.Id;
                    vlModel.CustomerId = customerId;
                }

                if (result > 0 || vlModel.IsSystem)
                {
                    Tb_PawnContract vlContract;
                    vlModel.DocumentDate = vlModel.DocumentDatePost;
                    vlModel.PawnDate = vlModel.PawnDatePost.Date;
                    if (vlModel.Id < 1) // Them moi record
                    {
                        vlContract = _mapper.Map<PawnContractModels, Tb_PawnContract>(vlModel);
                        _unitOfWork.PawnContractRepository.Insert(vlContract);
                        result = _unitOfWork.SaveChanges();
                        vlModel.Id = vlContract.Id;
                        if (result > 0)
                        {
                            // Khởi tạo các giá trị mới để add lại kỳ mới
                            ConvertMethod(vlModel);
                            HistoryModels historyClose = new HistoryModels
                            {
                                ContractID = vlModel.Id,
                                Content = "Cho vay",
                                StoreId = RDAuthorize.Store.Id,
                                TypeHistory = (int)DocumentTypeEnum.VayLai,
                                DebitMoney = (decimal)vlModel.TotalMoney,
                                HavingMoney = 0,
                                CreatedBy = RDAuthorize.Username,
                                CreatedDate = DateTime.Now
                            };
                            _historyServices.AddHistoryMessage(historyClose);
                            _cashBookServices.AddCashBook(new CashBookModals
                            {
                                CreatedDate = DateTime.Now,
                                CreatedUser = RDAuthorize.Username,
                                CreditAccount = 0,
                                DebitAccount = (decimal)vlModel.TotalMoney,
                                Note = Utility.GetDescriptionC6(NotesEnum.GiaiNganHopDongVayLai),
                                NoteId = (int)NotesEnum.GiaiNganHopDongVayLai,
                                Customer = vlModel.CustomerName,
                                DocumentDate = DateTime.Now,
                                DocumentType = (int)DocumentTypeEnum.VayLai,
                                VoucherType = (int)VocherTypeEnum.PhieuChi,
                                StoreId = RDAuthorize.Store.Id,
                                IsActive = true,
                                IsDeleted = false,
                                ContractId = historyClose.ContractID
                            });

                            if (vlModel.MoneyIntroduce.HasValue && vlModel.MoneyIntroduce > 0)
                                _cashBookServices.AddCashBook(new CashBookModals
                                {
                                    CreatedDate = DateTime.Now,
                                    CreatedUser = RDAuthorize.Username,
                                    CreditAccount = 0,
                                    DebitAccount = vlModel.MoneyIntroduce ?? 0,
                                    Note = "Tiền giới thiệu HD " + vlContract.Code,
                                    NoteId = (int)NotesEnum.TienGioiThieuHopDongVaiLai,
                                    DocumentDate = DateTime.Now,
                                    DocumentType = (int)DocumentTypeEnum.VayLai,
                                    VoucherType = (int)VocherTypeEnum.PhieuChi,
                                    StoreId = RDAuthorize.Store.Id,
                                    IsActive = true,
                                    Customer = vlModel.CustomerName,
                                    IsDeleted = false,
                                    ContractId = historyClose.ContractID
                                });

                            if (vlModel.MoneyServices.HasValue && vlModel.MoneyServices > 0)
                                _cashBookServices.AddCashBook(new CashBookModals
                                {
                                    CreatedDate = DateTime.Now,
                                    CreatedUser = RDAuthorize.Username,
                                    CreditAccount = vlModel.MoneyServices ?? 0,
                                    DebitAccount = 0,
                                    Customer = vlModel.CustomerName,
                                    Note = "Tiền dịch vụ HD " + vlContract.Code,
                                    NoteId = (int)NotesEnum.TienDichVuHopDongVayLai,
                                    DocumentDate = DateTime.Now,
                                    DocumentType = (int)DocumentTypeEnum.VayLai,
                                    VoucherType = (int)VocherTypeEnum.PhieuThu,
                                    StoreId = RDAuthorize.Store.Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    ContractId = historyClose.ContractID
                                });

                            message.Type = MessageTypeEnum.Success;
                            message.Message = "Thêm hợp đồng vay lãi thành công!";
                        }
                    }
                    else // Update record
                    {
                        vlContract = _unitOfWork.PawnContractRepository.Find(s => s.Id == vlModel.Id);

                        if (vlContract != null)
                        {
                            vlContract.CustomerId = vlModel.CustomerId;
                            vlContract.Code = vlModel.Code;
                            vlContract.Collateral = vlModel.Collateral;
                            vlContract.TotalMoney = vlModel.TotalMoney;
                            vlContract.InterestRateType = vlModel.InterestRateType;
                            vlContract.IsBefore = vlModel.IsBefore;
                            vlContract.InterestRate = vlModel.InterestRate;
                            vlContract.PawnDateNumber = vlModel.PawnDateNumber;
                            vlContract.InterestRateNumber = vlModel.InterestRateNumber;
                            vlContract.InterestRateOption = vlModel.InterestRateOption;
                            vlContract.PawnDate = vlModel.PawnDate;
                            vlContract.StaffManagerId = vlModel.StaffManagerId;

                            vlModel.DocumentDate = vlModel.DocumentDatePost;
                            vlModel.PawnDate = vlModel.PawnDatePost.Date;
                            vlModel.Note = vlModel.Note;
                            vlModel.UpdatedDate = DateTime.Now;
                            vlModel.UpdatedUser = RDAuthorize.Username;
                        }

                        var cus = _unitOfWork.CustomerRepository.Find(s => s.Id == vlContract.CustomerId);
                        if (cus != null)
                        {
                            cus.Fullname = vlModel.CustomerName;
                            cus.IdentityCard = vlModel.IdentityCard;
                            cus.IsActive = true;
                            cus.IsDeleted = false;
                            cus.Phone = vlModel.Phone;
                            cus.Address = vlModel.Address;
                            cus.ICCreatePlace = vlModel.ICCreatePlace;
                            cus.ICCreateDate = vlModel.ICCreateDate;
                            cus.UpdatedDate = DateTime.Now;
                            cus.UpdatedUser = RDAuthorize.Username;
                        }
                        result = _unitOfWork.SaveChanges();
                        var ms = UpdateHopDong(vlModel);
                        message = ms;
                    }
                }

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> AddBHContract ", ex);
            }

            return message;
        }

        public PawnContractModels LoadDetailPawnContract(int idContract)
        {
            PawnContractModels pawnContract;

            try
            {
                pawnContract = _unitOfWork.PawnContractRepository.Filter(s => s.Id == idContract)
                                                                 .Join(_unitOfWork.CustomerRepository.Filter(s => s.StoreId == RDAuthorize.Store.Id),
                                                                       p => p.CustomerId,
                                                                       cs => cs.Id,
                                                                       (p, cs) => new { p, cs })
                                                                 .Select(s => new PawnContractModels
                                                                 {
                                                                     IsSystem = true,
                                                                     CustomerId = s.p.CustomerId,
                                                                     CustomerName = s.cs.Fullname,
                                                                     Address = s.cs.Address,
                                                                     IdentityCard = s.cs.IdentityCard,
                                                                     ICCreateDate = s.cs.ICCreateDate,
                                                                     ICCreatePlace = s.cs.ICCreatePlace,
                                                                     Code = s.p.Code,
                                                                     Collateral = s.p.Collateral,
                                                                     DocumentDate = s.p.DocumentDate ?? DateTime.Now,
                                                                     PawnDate = s.p.PawnDate,
                                                                     DocumentName = s.p.DocumentName,
                                                                     DocumentType = s.p.DocumentType ?? 0,
                                                                     InterestRate = s.p.InterestRate,
                                                                     Id = s.p.Id,
                                                                     InterestRateNumber = s.p.InterestRateNumber,
                                                                     InterestRateOption = s.p.InterestRateOption,
                                                                     InterestRateType = s.p.InterestRateType,
                                                                     PawnDateNumber = s.p.PawnDateNumber,
                                                                     IsBefore = s.p.IsBefore,
                                                                     Note = s.p.Note,
                                                                     StoreId = s.p.StoreId,
                                                                     TotalMoney = s.p.TotalMoney,
                                                                     Phone = s.cs.Phone,
                                                                     MoneyIntroduce = s.p.MoneyIntroduce,
                                                                     MoneyServices = s.p.MoneyServices,
                                                                     StaffManagerId = s.p.StaffManagerId,
                                                                     IsCloseContract = s.p.IsCloseContract
                                                                 }).FirstOrDefault() ?? new PawnContractModels();
                //pawnContract.DocumentDateString = pawnContract.PawnDate.ToString(Constants.DateFormat);
                pawnContract.PawnDatePostString = pawnContract.PawnDate.ToString("dd-MM-yyyy");
                var pay = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == pawnContract.Id).ToList();
                if (pay.Count > 0)
                {
                    pawnContract.Status = StatusContract(pay, pawnContract.IsBefore);
                    pawnContract.ToDate = pay.Max(m => m.ToDate);
                    pawnContract.FromDate = pay.Min(m => m.FromDate);
                    pawnContract.TongLai = TongLai(pay);
                }
            }
            catch (Exception ex)
            {
                pawnContract = new PawnContractModels();
                PawnLog.Error("PawnServices --> LoadDetailPawnContract ", ex);
            }

            return pawnContract;

        }
        #endregion

        #region Code A Nam
        public IEnumerable<PawnContractModels> GetAllData(int storeId, DateTime? fromDate, DateTime? toDate,
         int currentPage, int pageSize, int? customerId, string docName, int? status, string sortcolumn, string sorttype, int staffId)
        {
            try
            {
                //var payList = _unitOfWork.PawnPayRepository.GetAllData();
                //var model = _unitOfWork.PawnContractRepository.Filter(x => x.CreatedDate >= fromDate
                //&& x.CreatedDate <= toDate
                //&& x.StoreId == storeId
                //&& (customerId == null || x.CustomerId == customerId)
                //&& (string.IsNullOrEmpty(docName) || x.Code == docName));
                //switch (status)
                //{
                //    case (int)StatusContractPawnEnum.HDDangvay:
                //        model = model.Where(m => !m.IsDeleted && !m.IsCloseContract);
                //        break;
                //    case (int)StatusContractPawnEnum.QuaHan:
                //model = from m in model
                //        where payList.Any(x=>(x.ContractId == m.Id) 
                //        && !x.IsPaid 
                //        &&  ( x.ToDate > DateTime.Now)
                //        //from m in model join p in payList on m.Id equals p.ContractId
                //        //     where !p.IsPaid group p by p into g
                //        //     where g.Max(x => x.ToDate > DateTime.Now
                //        && x(payList.Select)
                //        )

                // )            

                //var xmodel = from m in model.Any(x => (
                //        from a in payList where !a.IsPaid && a.ContractId == x.Id
                //        group a by a into g
                //        select g.Key.ContractId
                //        where g.Max(c => c.ToDate > DateTime.Now))
                //      )

                // )                 

                //model = model.Join(payList,
                //    m => m.Id,
                //    p => p.ContractId,
                //    (m, p) => new { m, p }).Where(x =>
                //    payList.)
                //    .Select(x => x.m);
                //        break;
                //    case (int)StatusContractPawnEnum.NoXau:
                //        model = model.Where(m => m.Status == (int)StatusContractPawnEnum.NoXau);
                //        break;
                //    case (int)StatusContractPawnEnum.KetThuc:
                //        model = model.Where(m => m.IsCloseContract);
                //        break;
                //    case (int)StatusContractPawnEnum.DaXoa:
                //        model = model.Where(m => m.IsDeleted);
                //        break;
                //    case (int)StatusContractPawnEnum.DungHen:
                //        var nowDate = DateTime.Now.AddDays(-3);
                //        model = model.Join(payList,
                //        m => m.Id,
                //        p => p.ContractId,
                //        (m, p) => new { m, p }).Where(x =>
                //        payList.Min(o => o.ContractId == x.m.Id
                //        && !o.IsPaid
                //        && !(x.m.IsBefore ? (o.ToDate.Date.AddDays(3) < DateTime.Now.Date) : (o.FromDate.Date.AddDays(3) < DateTime.Now.Date))))
                //        .Select(x => x.m);
                //        break;
                //    case (int)StatusContractPawnEnum.ChamLai:
                //        model = model.Join(payList,
                //            m => m.Id,
                //            p => p.ContractId,
                //            (m, p) => new { m, p }).Where(x =>
                //            payList.Min(o => o.ContractId == x.m.Id
                //            && !o.IsPaid
                //            && x.m.IsBefore ? (o.ToDate.Date.AddDays(3) < DateTime.Now.Date) : (o.FromDate.Date.AddDays(3) < DateTime.Now.Date)))
                //            .Select(x => x.m);
                //        break;

                //}
                //model = model.OrderByDescending(m => m.CreatedDate);
                //var data = model.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                var _params = new object[] { RDAuthorize.Store.Id, customerId, staffId, docName, status, fromDate, toDate, currentPage, pageSize, sortcolumn + " " + sorttype };
                var result = _unitOfWork.ExecStoreProdure<PawnContractModels>("SP_PAWN_SRH {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}", _params).ToList();
                // var result = _mapper.Map<IEnumerable<Tb_PawnContract>, IEnumerable<PawnContractModels>>(data);
                //    var pawnContractModelses = result.ToList();
                result = result.Skip((currentPage - 1) * pageSize).Take(pageSize).ToList();
                foreach (var item in result)
                {
                    var notification = _unitOfWork.TimerRepository.Filter(m => m.ContractId == item.Id
                    && m.DocumentType == (int)DocumentTypeEnum.VayLai);
                    var pay = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == item.Id).ToList();
                    var debt = _unitOfWork.CapitalLoanRepository.Filter(m => m.CapitalId == item.Id
                                && m.DocumentType == (int)DocumentTypeEnum.VayLai).ToList();
                    double vaythem = 0; double tragoc = 0;
                    if (debt.Any())
                    {
                        vaythem = (double)debt.Where(m => m.IsLoan).Sum(m => m.MoneyNumber);
                        tragoc = (double)debt.Where(m => !m.IsLoan).Sum(m => m.MoneyNumber);
                    }
                    if (pay.Count > 0)
                    {
                        item.LaiDaDong = MoneyIsPay(pay);
                        item.NgayLaiDaDong = DayIsPay(pay);
                        item.NgayLaiHomNay = DayIsPayNow(pay, item.IsBefore);
                        item.LaiDenNgay = MoneyIsPayNow(item, pay, vaythem - tragoc);
                        item.NoCu = DebitOld(pay);
                        item.Status = StatusContract(pay, item.IsBefore);
                        item.NgayPhaiDongLai = NgayPhaiDongLai(pay, item.IsBefore);
                        item.ToDate = pay.Max(m => m.ToDate);
                        item.FromDate = pay.Min(m => m.FromDate);
                        item.TongLai = TongLai(pay);
                        item.TotalMoney = item.TotalMoney;
                        item.TienVayThem = vaythem - tragoc;
                        item.IsNotification = notification.Any();
                        item.IsPaid = pay.Any(s => s.IsPaid == true);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> GetAllData ", ex);
                return new List<PawnContractModels>();
            }
        }

        public PawnContractModels GetDetailById(int storeId, int contractId)
        {
            try
            {
                var data = _unitOfWork.PawnContractRepository.Find(x => x.Id == contractId && x.StoreId == storeId);
                var result = _mapper.Map<Tb_PawnContract, PawnContractModels>(data);
                result.CustomerName = _unitOfWork.CustomerRepository.Filter(m => m.Id == result.CustomerId)
                    .FirstOrDefault()
                    ?.Fullname;
                var pay = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == result.Id).ToList();
                var debt = _unitOfWork.CapitalLoanRepository.Filter(m => m.CapitalId == result.Id
                && m.DocumentType == (int)DocumentTypeEnum.VayLai).ToList();
                double vaythem = 0; double tragoc = 0;
                if (debt.Any())
                {
                    vaythem = (double)debt.Where(m => m.IsLoan).Sum(m => m.MoneyNumber);
                    tragoc = (double)debt.Where(m => !m.IsLoan).Sum(m => m.MoneyNumber);
                }
                if (pay.Count > 0)
                {
                    var toDate = pay.Max(m => m.ToDate);
                    result.LaiDaDong = MoneyIsPay(pay);
                    result.NgayLaiDaDong = DayIsPay(pay);
                    result.NgayLaiHomNay = DayIsPayNow(pay, result.IsBefore);
                    result.LaiDenNgay = MoneyIsPayNow(result, pay, vaythem - tragoc);
                    result.TienLaiMotNgay = MoneyByDay(result, vaythem - tragoc);
                    result.NoCu = DebitOld(pay);
                    result.Status = StatusContract(pay, result.IsBefore);
                    result.NgayPhaiDongLai = NgayPhaiDongLai(pay, result.IsBefore);
                    result.ToDate = toDate;
                    result.FromDate = pay.Min(m => m.FromDate);
                    result.TongLai = TongLai(pay);
                    result.TotalMoney = result.TotalMoney;
                    result.TienVayThem = vaythem - tragoc;
                    //result.PawnDatePostString = result.PawnDateString;
                    result.IsPaid = pay.Any(s => s.IsPaid == true);
                    var lstExtentionContract = _extentionContractServices.LoadDataExtentionContract(result.Id, DocumentTypeEnum.VayLai);
                    result.ListExtentionContract = LoadListExtentionContract(lstExtentionContract, result.InterestRateType ?? 0);
                }
                return result;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> GetDetailById ", ex);
                return new PawnContractModels();
            }
        }

        // Get Pawns By Id
        public IEnumerable<PawnPayModels> GetPawnPaysById(int id)
        {
            try
            {
                var data = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == id).OrderBy(m => m.FromDate).ToList();
                var result = _mapper.Map<IEnumerable<Tb_PawnPay>, IEnumerable<PawnPayModels>>(data).ToList();
                var now = result.FirstOrDefault(m => !m.IsPaid);
                if (now != null)
                {
                    var index = result.FindIndex(n => n == now);
                    result[index].LoanDate = DateTime.Now;
                    result[index].IsCurrent = true;
                    result[index].CustomerMoney = now.InterestMoney;
                }
                return result;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnIncomeAndExpenseServices --> GetAllData ", ex);
                return new List<PawnPayModels>();
            }
        }
        // tiền lãi đã đóng
        private double MoneyIsPay(List<Tb_PawnPay> paid)
        {
            var money = paid.Where(m => m.IsPaid).Sum(x => x.CustomerMoney);
            return Convert.ToDouble(money);
        }
        // ngày lãi đã đóng
        private double DayIsPay(List<Tb_PawnPay> paid)
        {
            var days = paid.Where(m => m.IsPaid).Sum(x => x.TotalDay);
            return days ?? 0;
        }

        // tiền lãi hôm nay
        private double MoneyIsPayNow(PawnContractModels item, List<Tb_PawnPay> paid, double totalCapital)
        // ngay lãi hôm nay, phương thức, loại , tiền lãi, tổng tiền vay
        {
            var pay = paid.Where(m => !m.IsPaid).ToList();
            DateTime beginDate;
            DateTime endDate;
            if (pay.Any())
            {
                beginDate = pay.Min(m => m.FromDate);
                endDate = pay.Min(m => m.ToDate);
            }
            else
            {
                beginDate = DateTime.Now;
                endDate = DateTime.Now;
            }
            var day = DateTime.Now.Date - Convert.ToDateTime(item.IsBefore ? beginDate : endDate);
            var ngayLaiHomNay = day.TotalDays;
            if (ngayLaiHomNay <= 0) return 0;
            switch (item.InterestRateType) //method
            {
                case (int)MethodTypeEnum.MoneyPerDay:
                    {
                        if (item.InterestRateOption == (int)CapitalRateTypeEnum.KPerMillion)
                        {
                            return ((item.TotalMoney + totalCapital) / 1000000) * item.InterestRate * ngayLaiHomNay;
                        }
                        else
                        {
                            return item.InterestRate * ngayLaiHomNay;
                        }

                    }
                case (int)MethodTypeEnum.MoneyPerWeek:
                    {
                        return (item.InterestRate / 7) * ngayLaiHomNay;
                    }

                case (int)MethodTypeEnum.PercentPerMonth30:
                    {
                        return (item.InterestRate * (item.TotalMoney + totalCapital) / 100) / 30 * ngayLaiHomNay;
                    }
                case (int)MethodTypeEnum.PercentPerMonthDay:
                    {
                        var toDate = item.PawnDate.AddMonths(1);
                        var payDate = (toDate - item.PawnDate).TotalDays;
                        return (item.InterestRate * (item.TotalMoney + totalCapital) / 100) / payDate * ngayLaiHomNay;
                    }
                case (int)MethodTypeEnum.PercentPerWeek:
                    {
                        return (item.InterestRate * (item.TotalMoney + totalCapital) / 100) / 7 * ngayLaiHomNay;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        // tiền lãi một ngày
        private double MoneyByDay(PawnContractModels item, double totalCapital)
        // ngay lãi hôm nay, phương thức, loại , tiền lãi, tổng tiền vay
        {
            switch (item.InterestRateType) //method
            {
                case (int)MethodTypeEnum.MoneyPerDay:
                    {
                        if (item.InterestRateOption == (int)CapitalRateTypeEnum.KPerMillion)
                        {
                            return ((item.TotalMoney + totalCapital) / 1000000) * item.InterestRate;
                        }
                        else
                        {
                            return item.InterestRate;
                        }

                    }
                case (int)MethodTypeEnum.MoneyPerWeek:
                    {
                        return (item.InterestRate / 7);
                    }

                case (int)MethodTypeEnum.PercentPerMonth30:
                    {
                        return (item.InterestRate * (item.TotalMoney + totalCapital) / 100) / 30;
                    }
                case (int)MethodTypeEnum.PercentPerMonthDay:
                    {
                        var toDate = item.PawnDate.AddMonths(1);
                        var payDate = (toDate - item.PawnDate).TotalDays;
                        return (item.InterestRate * (item.TotalMoney + totalCapital) / 100) / payDate;
                    }
                case (int)MethodTypeEnum.PercentPerWeek:
                    {
                        return (item.InterestRate * (item.TotalMoney + totalCapital) / 100) / 7;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        // ngày lãi  hôm nay
        private double DayIsPayNow(List<Tb_PawnPay> paid, bool isBefore)
        {
            var pay = paid.Where(m => !m.IsPaid);
            DateTime beginDate;
            DateTime endDate;
            if (pay.Count() > 0)
            {
                beginDate = pay.Min(m => m.FromDate);
                endDate = pay.Min(m => m.ToDate);
            }
            else
            {
                beginDate = DateTime.Now;
                endDate = DateTime.Now;
            }
            var days = DateTime.Now.Date - Convert.ToDateTime(isBefore ? beginDate : endDate);
            return days.TotalDays;
        }


        //Nợ cũ
        private decimal DebitOld(List<Tb_PawnPay> paid)
        {
            var pay = paid.Where(m => m.IsPaid).ToList();
            return ((pay.Sum(x => x.InterestMoney) ?? 0) - (pay.Sum(x => x.CustomerMoney) ?? 0));
        }

        //tình trạng
        private int StatusContract(List<Tb_PawnPay> paid, bool isBefore)
        {
            var day = paid.Where(m => !m.IsPaid);
            if (day.Count() > 0)
            {
                if (day.Max(x => x.ToDate) == DateTime.Now.Date) return (int)PawnStatusEnum.TraGoc;
                else if (day.Max(x => x.ToDate) < DateTime.Now.Date) return (int)PawnStatusEnum.QuaHan;
                else if ((isBefore) ? day.Min(x => x.FromDate) < DateTime.Now : day.Min(x => x.ToDate) < DateTime.Now) return (int)PawnStatusEnum.NoLai;
                else return (int)PawnStatusEnum.DangVay;
            }
            else return (int)PawnStatusEnum.DangVay;

        }

        private DateTime NgayPhaiDongLai(List<Tb_PawnPay> paid, bool isBefore)
        {
            var day = paid.Where(m => !m.IsPaid);
            if (day.Count() > 0)
            {
                return (isBefore) ? day.Min(x => x.FromDate) : day.Min(x => x.ToDate);// thu lãi trước thì lấy todate
            }
            else
            {
                return Convert.ToDateTime("01/01/1900");
            }

        }

        // ABC 
        private void ConvertMethod(PawnContractModels model)
        {
            var pay = new Pay
            {
                NgayBatDau = model.PawnDate,  // ngày bắt đầu vay
                SoLanTraChan = model.PawnDateNumber / model.InterestRateNumber ?? 0,
                SoLanTraLe = model.PawnDateNumber % model.InterestRateNumber ?? 0,
                SoNgayVay = model.PawnDateNumber ?? 0,
                TienLai = model.InterestRate * model.InterestRateNumber * 1000 ?? 0, // 1K * 1000
                ValueMethod = 1
            };
            switch (model.InterestRateType) //method
            {
                case (int)MethodTypeEnum.MoneyPerDay:
                    {
                        if (model.InterestRateOption == 0)
                        {
                            pay.ValueMethod = 1;
                            pay.SoNgayVay = model.PawnDateNumber ?? 0;
                            pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 1000000) * model.InterestRateNumber * 1000 ?? 0;
                        }
                        break;
                    }
                case (int)MethodTypeEnum.MoneyPerWeek:
                    {
                        pay.ValueMethod = 7;
                        pay.SoNgayVay = model.PawnDateNumber ?? 0 * 7;
                        pay.TienLai = model.InterestRate * model.InterestRateNumber * 1000 ?? 0;
                        break;
                    }

                case (int)MethodTypeEnum.PercentPerMonth30:
                    {
                        pay.ValueMethod = 30;
                        pay.SoNgayVay = model.PawnDateNumber ?? 0 * 30;
                        pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 100) * model.InterestRateNumber ?? 0;
                        break;
                    }
                case (int)MethodTypeEnum.PercentPerMonthDay:
                    {
                        pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 100) * model.InterestRateNumber ?? 0;
                        break;
                    }
                case (int)MethodTypeEnum.PercentPerWeek:
                    {
                        pay.ValueMethod = 7;
                        pay.SoNgayVay = model.PawnDateNumber ?? 0 * 7;
                        pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 100) * model.InterestRateNumber ?? 0;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            #region  Dinh Ky Thang
            if (model.InterestRateType == (int)MethodTypeEnum.PercentPerMonthDay)
            {
                var tampDate = pay.NgayBatDau.AddDays(0);
                if (pay.SoLanTraChan > 0)
                {
                    for (int i = 1; i <= pay.SoLanTraChan; i++)
                    {
                        var toDate = tampDate.AddMonths(model.InterestRateNumber ?? 0);
                        var payDate = (int)(toDate - tampDate).TotalDays;
                        var p = new PawnPayModels
                        {
                            ContractId = model.Id,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CustomerMoney = 0,
                            IsPaid = false,
                            FromDate = tampDate,
                            ToDate = toDate,
                            InterestMoney = (decimal)pay.TienLai,
                            OtherMoney = 0,
                            TotalDay = payDate + 1
                        };
                        tampDate = toDate;
                        AddNewPawnPay(p);
                    }
                }
                if (pay.SoLanTraLe > 0)
                {
                    var payDate = (int)(tampDate.AddMonths(pay.SoLanTraLe) - tampDate).TotalDays;
                    var p = new PawnPayModels
                    {
                        ContractId = model.Id,
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CustomerMoney = 0,
                        IsPaid = false,
                        FromDate = tampDate,
                        ToDate = tampDate.AddDays(payDate),
                        InterestMoney = (decimal)(pay.TienLai * pay.SoLanTraLe / model.InterestRateNumber),
                        OtherMoney = 0,
                        TotalDay = payDate + 1 //đéo hiểu sau +1 nữa
                        // LoanDate: ngày trả
                    };
                    AddNewPawnPay(p);
                }
            }
            #endregion
            else
            {
                var step = 0;
                if (pay.SoLanTraChan > 0)
                {
                    for (int i = 1; i <= pay.SoLanTraChan; i++)
                    {
                        var p = new PawnPayModels
                        {
                            ContractId = model.Id,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CustomerMoney = 0,
                            IsPaid = false,
                            FromDate = pay.NgayBatDau.AddDays(step),
                            ToDate = pay.NgayBatDau.AddDays(step + (int)model.InterestRateNumber * pay.ValueMethod - 1),
                            InterestMoney = (decimal)pay.TienLai,
                            OtherMoney = 0,
                            TotalDay = (int)model.InterestRateNumber * pay.ValueMethod
                        };
                        step += (int)model.InterestRateNumber * pay.ValueMethod;
                        AddNewPawnPay(p);
                    }
                }
                if (pay.SoLanTraLe > 0)
                {
                    var p = new PawnPayModels
                    {
                        ContractId = model.Id,
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CustomerMoney = 0,
                        IsPaid = false,
                        FromDate = pay.NgayBatDau.AddDays(step),
                        ToDate = pay.NgayBatDau.AddDays(step + pay.SoLanTraLe * pay.ValueMethod - 1),
                        InterestMoney = (decimal)(pay.TienLai * pay.SoLanTraLe / model.InterestRateNumber),
                        OtherMoney = 0,
                        TotalDay = pay.SoLanTraLe * pay.ValueMethod
                        // LoanDate: ngày trả
                    };
                    AddNewPawnPay(p);
                }
            }
            //   return pay;
        }
        private void ConvertMethodOptimize(PawnContractModels model, int pawnDayNumber, int valueMethod)
        {
            var pay = new Pay
            {
                NgayBatDau = model.PawnDate,  // ngày bắt đầu vay
                SoLanTraChan = pawnDayNumber / (model.InterestRateNumber * valueMethod) ?? 0,
                SoLanTraLe = pawnDayNumber % (model.InterestRateNumber * valueMethod) ?? 0,
                SoNgayVay = model.PawnDateNumber ?? 0,
                TienLai = model.InterestRate * model.InterestRateNumber * 1000 ?? 0, // 1K * 1000
                ValueMethod = 1
            };
            switch (model.InterestRateType) //method
            {
                case (int)MethodTypeEnum.MoneyPerDay:
                    {
                        if (model.InterestRateOption == 0)
                        {
                            pay.ValueMethod = 1;
                            pay.SoNgayVay = model.PawnDateNumber ?? 0;
                            pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 1000000) * model.InterestRateNumber * 1000 ?? 0;
                        }
                        break;
                    }
                case (int)MethodTypeEnum.MoneyPerWeek:
                    {
                        pay.ValueMethod = 7;
                        pay.SoNgayVay = model.PawnDateNumber ?? 0 * 7;
                        pay.TienLai = model.InterestRate * model.InterestRateNumber * 1000 ?? 0;
                        break;
                    }

                case (int)MethodTypeEnum.PercentPerMonth30:
                    {
                        pay.ValueMethod = 30;
                        pay.SoNgayVay = model.PawnDateNumber ?? 0 * 30;
                        pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 100) * model.InterestRateNumber ?? 0;
                        break;
                    }
                case (int)MethodTypeEnum.PercentPerMonthDay:
                    {
                        pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 100) * model.InterestRateNumber ?? 0;
                        break;
                    }
                case (int)MethodTypeEnum.PercentPerWeek:
                    {
                        pay.ValueMethod = 7;
                        pay.SoNgayVay = model.PawnDateNumber ?? 0 * 7;
                        pay.TienLai = ((model.TotalMoney + model.TienVayThem) * model.InterestRate / 100) * model.InterestRateNumber ?? 0;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            #region  Dinh Ky Thang
            if (model.InterestRateType == (int)MethodTypeEnum.PercentPerMonthDay)
            {
                var tampDate = pay.NgayBatDau.AddDays(0);
                if (pay.SoLanTraChan > 0)
                {
                    for (int i = 1; i <= pay.SoLanTraChan; i++)
                    {
                        var toDate = tampDate.AddMonths(model.InterestRateNumber ?? 0);
                        var payDate = (int)(toDate - tampDate).TotalDays;
                        var p = new PawnPayModels
                        {
                            ContractId = model.Id,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CustomerMoney = 0,
                            IsPaid = false,
                            FromDate = tampDate,
                            ToDate = toDate,
                            InterestMoney = (decimal)pay.TienLai,
                            OtherMoney = 0,
                            TotalDay = payDate + 1
                        };
                        tampDate = toDate;
                        AddNewPawnPay(p);
                    }
                }
                if (pay.SoLanTraLe > 0)
                {
                    var payDate = pay.SoLanTraLe;
                    var moneyDate = (model.TotalMoney + model.TienVayThem) * model.InterestRate / 100 / 30;
                    var p = new PawnPayModels
                    {
                        ContractId = model.Id,
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CustomerMoney = 0,
                        IsPaid = false,
                        FromDate = tampDate,
                        ToDate = tampDate.AddDays(payDate),
                        InterestMoney = (decimal)((pay.SoLanTraLe + 1) * moneyDate),
                        OtherMoney = 0,
                        TotalDay = payDate + 1//đéo hiểu sau +1 nữa
                        // LoanDate: ngày trả
                    };
                    AddNewPawnPay(p);
                }
            }
            #endregion
            else
            {
                var step = 0;
                if (pay.SoLanTraChan > 0)
                {
                    for (int i = 1; i <= pay.SoLanTraChan; i++)
                    {
                        var p = new PawnPayModels
                        {
                            ContractId = model.Id,
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CustomerMoney = 0,
                            IsPaid = false,
                            FromDate = pay.NgayBatDau.AddDays(step),
                            ToDate = pay.NgayBatDau.AddDays(step + ((int)model.InterestRateNumber * pay.ValueMethod) - 1),
                            InterestMoney = (decimal)pay.TienLai,
                            OtherMoney = 0,
                            TotalDay = (int)model.InterestRateNumber * pay.ValueMethod
                        };
                        step += (int)model.InterestRateNumber * pay.ValueMethod;
                        AddNewPawnPay(p);
                    }
                }
                if (pay.SoLanTraLe > 0)
                {
                    var p = new PawnPayModels
                    {
                        ContractId = model.Id,
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CustomerMoney = 0,
                        IsPaid = false,
                        FromDate = pay.NgayBatDau.AddDays(step),
                        ToDate = pay.NgayBatDau.AddDays(step + (pay.SoLanTraLe) - 1),
                        InterestMoney = (decimal)(pay.TienLai * pay.SoLanTraLe / valueMethod / model.InterestRateNumber),
                        OtherMoney = 0,
                        TotalDay = pay.SoLanTraLe
                        // LoanDate: ngày trả
                    };
                    AddNewPawnPay(p);
                }
            }
            //   return pay;
        }
        // Chuyển đổi lãi các kỳ thành đơn vị cơ bản
        //private Pay ConvertMethodToBase(PawnContractModels model, double totalCaptial)
        //{
        //    var pay = new Pay
        //    {
        //        NgayBatDau = model.PawnDate,
        //        SoLanTra = model.InterestRateNumber ?? 0,
        //        SoNgayVay = model.PawnDateNumber ?? 0,
        //        TienLai = model.InterestRate  // tiên lãi cho 1 ngày
        //    };
        //    switch (model.InterestRateType) //method
        //    {
        //        case (int)MethodTypeEnum.MoneyPerDay:
        //            {
        //                if (model.InterestRateOption == (int)CapitalRateTypeEnum.KPerMillion)
        //                {
        //                    pay.SoLanTra = model.InterestRateNumber ?? 0;
        //                    pay.SoNgayVay = model.PawnDateNumber ?? 0;
        //                    pay.TienLai = ((model.TotalMoney + totalCaptial) * model.InterestRate / 1000000); //tiên lãi cho 1 ngày
        //                }
        //                break;
        //            }
        //        case (int)MethodTypeEnum.MoneyPerWeek:
        //            {
        //                pay.SoLanTra = (decimal)model.InterestRateNumber / 7;
        //                pay.SoNgayVay = (decimal)model.PawnDateNumber / 7;
        //                pay.TienLai = model.InterestRate / 7; //tiên lãi cho 1 ngày
        //                break;
        //            }

        //        case (int)MethodTypeEnum.PercentPerMonth30:
        //            {
        //                pay.SoLanTra = (decimal)model.InterestRateNumber / 30;
        //                pay.SoNgayVay = (decimal)model.PawnDateNumber / 30;
        //                pay.TienLai = (model.InterestRate * (model.TotalMoney + totalCaptial) / 100) / 30; //tiên lãi cho 1 ngày
        //                break;
        //            }
        //        case (int)MethodTypeEnum.PercentPerMonthDay:
        //            {
        //                var toDate = model.PawnDate.AddMonths(1);
        //                var payDate = (int)(toDate - model.PawnDate).TotalDays;
        //                pay.SoLanTra = (decimal)model.InterestRateNumber / payDate;
        //                pay.SoNgayVay = (decimal)model.PawnDateNumber / payDate;
        //                pay.TienLai = (model.InterestRate * (model.TotalMoney + totalCaptial) / 100) / payDate; //tiên lãi cho 1 ngày
        //                break;
        //            }
        //        case (int)MethodTypeEnum.PercentPerWeek:
        //            {
        //                pay.SoLanTra = (decimal)model.InterestRateNumber / 7;
        //                pay.SoNgayVay = (decimal)model.PawnDateNumber / 7;
        //                pay.TienLai = (model.InterestRate * (model.TotalMoney + totalCaptial) / 100) / 7; //tiên lãi cho 1 ngày
        //                break;
        //            }
        //        default:
        //            {
        //                break;
        //            }

        //    }
        //    return pay;
        //}

        //// Generate các kỳ lãi
        //private void GeneratePeriodPay(Pay pay, int contractId, bool isBefore)
        //{
        //    //tinh pay detail
        //    int soNgayChan = pay.SoLanTraChan;
        //    int soNgayLe = pay.SoLanTraLe;
        //    int step = isBefore ? 0 : 1;
        //    if (soNgayChan > 0)
        //    {
        //        for (int i = 1; i <= soNgayChan; i++)
        //        {
        //            var p = new PawnPayModels
        //            {
        //                ContractId = contractId,
        //                CreatedDate = DateTime.Now,
        //                CreatedUser = RDAuthorize.Username,
        //                CustomerMoney = 0,
        //                IsPaid = false,
        //                FromDate = pay.NgayBatDau.AddDays(step),
        //                ToDate = pay.NgayBatDau.AddDays(step + soNgayChan - 1),
        //                InterestMoney = (decimal)pay.TienLai * pay.SoLanTraChan,
        //                OtherMoney = 0,
        //                TotalDay = soNgayChan //pay.SoLanTra
        //                // LoanDate: ngày trả
        //            };
        //            step++;
        //            AddNewPawnPay(p);
        //        }
        //    }

        //    if (soNgayLe > 0)
        //    {
        //        var p = new PawnPayModels
        //        {
        //            ContractId = contractId,
        //            CreatedDate = DateTime.Now,
        //            CreatedUser = RDAuthorize.Username,
        //            CustomerMoney = 0,
        //            IsPaid = false,
        //            FromDate = pay.NgayBatDau.AddDays(step),
        //            ToDate = pay.NgayBatDau.AddDays(step + soNgayLe - 1),
        //            InterestMoney = (decimal)(pay.TienLai * soNgayLe),
        //            OtherMoney = 0,
        //            TotalDay = soNgayLe
        //            // LoanDate: ngày trả
        //        };
        //        step++;
        //        AddNewPawnPay(p);
        //    }
        //}
        // tổng lãi
        private double TongLai(List<Tb_PawnPay> paid)
        {
            var money = paid.Sum(x => x.OtherMoney) + paid.Sum(x => x.InterestMoney);
            return Convert.ToDouble(money);
        }
        #endregion

        public int AddNewPawnPay(PawnPayModels model)
        {
            model.FromDate = model.FromDate.Date;
            model.ToDate = model.ToDate.Date;
            var data = _mapper.Map<PawnPayModels, Tb_PawnPay>(model);
            _unitOfWork.PawnPayRepository.Insert(data);
            return _unitOfWork.SaveChanges();
        }

        public int UpdatePawnPay(PawnPayModels model)
        {
            model.FromDate = model.FromDate.Date;
            model.ToDate = model.ToDate.Date;
            var data = _mapper.Map<PawnPayModels, Tb_PawnPay>(model);
            _unitOfWork.PawnPayRepository.Update(data);
            return _unitOfWork.SaveChanges();
        }

        public int DeletePawnPay(int contractId)
        {
            var data = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == contractId
            && !m.IsPaid).ToList();
            foreach (var item in data)
            {
                _unitOfWork.PawnPayRepository.Delete(item.Id);
            }
            return _unitOfWork.SaveChanges();
        }

        public MessageModels DongLai(PawnContractModels header, PawnPayModels line)
        {
            header.PawnDate = header.PawnDatePost;
            header.DocumentDate = header.DocumentDatePost;
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"{(line.IsPaid ? "Thanh" : "Hủy")} toán từ ngày {line.FromDate:dd-MM-yyyy} đến ngày {line.ToDate:dd-MM-yyyy} thất bại!"
            };
            try
            {
                if (line.IsPaid) // Dong Lai
                {
                    var pay = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == line.ContractId
                    && !m.IsPaid && m.FromDate <= line.FromDate).ToList();
                    if (pay.Count() > 1)
                    {
                        //  var pay = _mapper.Map<List<Tb_PawnPay>, List<PawnPayModels>>(payModel);
                        // pay.Add(model);
                        foreach (var item in pay)
                        {
                            item.IsPaid = true;
                            item.LoanDate = DateTime.Now;
                            item.CustomerMoney = item.InterestMoney;
                            _unitOfWork.PawnPayRepository.Update(item);
                            HistoryModels historyClose = new HistoryModels
                            {
                                ContractID = item.ContractId,
                                Content = "Đóng lãi",
                                StoreId = RDAuthorize.Store.Id,
                                TypeHistory = (int)DocumentTypeEnum.VayLai,
                                DebitMoney = 0,
                                HavingMoney = (decimal)item.InterestMoney,
                                CreatedBy = RDAuthorize.Username,
                                CreatedDate = DateTime.Now
                            };
                            _historyServices.AddHistoryMessage(historyClose);

                            _cashBookServices.AddCashBook(new CashBookModals
                            {
                                CreatedDate = DateTime.Now,
                                CreatedUser = RDAuthorize.Username,
                                CreditAccount = (decimal)item.InterestMoney,
                                DebitAccount = 0,
                                Note = "Đóng lãi",
                                NoteId = (int)NotesEnum.DongLai,
                                Customer = header.CustomerName,
                                DocumentDate = DateTime.Now,
                                DocumentType = (int)DocumentTypeEnum.VayLai,
                                VoucherType = (int)VocherTypeEnum.PhieuThu,
                                StoreId = RDAuthorize.Store.Id,
                                IsActive = true,
                                IsDeleted = false,
                                ContractId = historyClose.ContractID
                            });
                        }
                        _unitOfWork.SaveChanges();
                        message.Type = MessageTypeEnum.Success;
                        message.Message = $"Thanh toán {pay.Count()} kỳ thành công!";
                    }
                    else
                    {
                        pay[0].LoanDate = DateTime.Now;
                        pay[0].IsPaid = true;
                        pay[0].CustomerMoney = line.CustomerMoney;
                        _unitOfWork.PawnPayRepository.Update(pay[0]);
                        HistoryModels historyClose = new HistoryModels
                        {
                            ContractID = line.ContractId,
                            Content = "Đóng lãi",
                            StoreId = RDAuthorize.Store.Id,
                            TypeHistory = (int)DocumentTypeEnum.VayLai,
                            DebitMoney = 0,
                            HavingMoney = (decimal)line.CustomerMoney,
                            CreatedBy = RDAuthorize.Username,
                            CreatedDate = DateTime.Now
                        };
                        _historyServices.AddHistoryMessage(historyClose);
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CreditAccount = (decimal)line.CustomerMoney,
                            DebitAccount = 0,
                            Note = "Đóng lãi",
                            NoteId = (int)NotesEnum.DongLai,
                            Customer = header.CustomerName,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.VayLai,
                            VoucherType = (int)VocherTypeEnum.PhieuThu,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            IsDeleted = false,
                            ContractId = historyClose.ContractID
                        });
                        _unitOfWork.SaveChanges();
                        message.Type = MessageTypeEnum.Success;
                        message.Message = $"Thanh toán từ ngày {line.FromDate:dd-MM-yyyy} đến ngày {line.ToDate:dd-MM-yyyy} thành công!";
                    }
                }
                else //Huy Dong Lai
                {
                    var debitAccount = line.CustomerMoney;
                    line.LoanDate = null;
                    line.CustomerMoney = 0;
                    line.IsPaid = false;
                    line.OtherMoney = 0;
                    UpdatePawnPay(line);
                    HistoryModels historyClose = new HistoryModels
                    {
                        ContractID = line.ContractId,
                        Content = "Hủy đóng lãi",
                        StoreId = RDAuthorize.Store.Id,
                        TypeHistory = (int)DocumentTypeEnum.VayLai,
                        DebitMoney = debitAccount,
                        HavingMoney = 0,
                        CreatedBy = RDAuthorize.Username,
                        CreatedDate = DateTime.Now
                    };
                    _historyServices.AddHistoryMessage(historyClose);
                    _cashBookServices.AddCashBook(new CashBookModals
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CreditAccount = 0,
                        Note = "Hủy đóng lãi",
                        NoteId = (int)NotesEnum.HuyDongLai,
                        Customer = header.CustomerName,
                        DebitAccount = (decimal)debitAccount,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.VayLai,
                        VoucherType = (int)VocherTypeEnum.PhieuChi,
                        StoreId = RDAuthorize.Store.Id,
                        IsActive = true,
                        IsDeleted = false,
                        ContractId = historyClose.ContractID
                    });

                    //khởi tạo lại
                    DeletePawnPay(header.Id);
                    // Khởi tạo các giá trị mới để add lại kỳ mới
                    //*************gán giá trị mới**********
                    var detail = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == header.Id && m.IsPaid);
                    if (detail.Any())
                    {
                        header.PawnDate = detail.Max(m => m.ToDate).AddDays(1);
                        var songaydadong = detail.Sum(m => m.TotalDay) ?? 0; // songayvay - songaydadong
                        switch (header.InterestRateType)
                        {
                            case (int)MethodTypeEnum.PercentPerMonthDay:
                                header.PawnDate = detail.Max(m => m.ToDate);
                                songaydadong = songaydadong / 30;
                                break;
                            case (int)MethodTypeEnum.PercentPerMonth30:
                                songaydadong = songaydadong / 30;
                                break;
                            case (int)MethodTypeEnum.PercentPerWeek:
                            case (int)MethodTypeEnum.MoneyPerWeek:
                                songaydadong = songaydadong / 7;
                                break;
                            default:
                                break;
                        }
                        header.PawnDateNumber -= songaydadong;
                    }
                    ConvertMethod(header);
                    message.Type = MessageTypeEnum.Success;
                    message.Message = $"Hủy thanh toán từ ngày {line.FromDate:dd-MM-yyyy} đến ngày {line.ToDate:dd-MM-yyyy} thành công!";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnService >>>> DongLai", ex);
            }

            return message;
            // thêm lịch sữ
        }

        public MessageModels DongLaiTuyBien(PawnContractModels header, PawnPayModels line)
        {
            header.PawnDate = header.PawnDatePost;
            header.DocumentDate = header.DocumentDatePost;
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Thanh toán từ ngày {line.FromDate:dd-MM-yyyy} đến ngày {line.ToDate:dd-MM-yyyy} thất bại!"
            };
            try
            {
                // lấy ngày chưa thanh toán
                var payBefore = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == header.Id
                && !m.IsPaid && m.FromDate == line.FromDate).FirstOrDefault();
                if (payBefore != null)
                {
                    payBefore.IsPaid = true;
                    payBefore.LoanDate = DateTime.Now;
                    payBefore.OtherMoney = line.OtherMoney;
                    payBefore.InterestMoney = line.InterestMoney * line.TotalDay;
                    payBefore.CustomerMoney = line.InterestMoney * line.TotalDay;
                    payBefore.TotalDay = line.TotalDay;
                    payBefore.ToDate = line.ToDate;
                    payBefore.FromDate = line.FromDate;
                    _unitOfWork.PawnPayRepository.Update(payBefore);
                    _unitOfWork.SaveChanges();
                    _historyServices.AddHistoryMessage(new HistoryModels
                    {
                        ContractID = payBefore.ContractId,
                        Content = "Đóng lãi",
                        StoreId = RDAuthorize.Store.Id,
                        TypeHistory = (int)DocumentTypeEnum.VayLai,
                        DebitMoney = 0,
                        HavingMoney = (decimal)payBefore.InterestMoney,
                        CreatedBy = RDAuthorize.Username,
                        CreatedDate = DateTime.Now
                    });

                    _cashBookServices.AddCashBook(new CashBookModals
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CreditAccount = (decimal)payBefore.InterestMoney,
                        DebitAccount = 0,
                        Note = "Đóng lãi",
                        NoteId = (int)NotesEnum.DongLai,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.VayLai,
                        VoucherType = (int)VocherTypeEnum.PhieuThu,
                        StoreId = RDAuthorize.Store.Id,
                        IsActive = true,
                        Customer = header.CustomerName,
                        IsDeleted = false,
                        ContractId = payBefore.ContractId
                    });
                }

                // Đóng lãi tùy biến
                //    AddNewPawnPay(line);
                // Remove các kỳ chưa đóng lãi để tạo lại các kỳ
                DeletePawnPay(header.Id);
                // Khởi tạo các giá trị mới để add lại kỳ mới
                //*************gán giá trị mới**********
                var songaydadong = 0; // songayvay - songaydadong
                int valueMethod = 1;
                var detail = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == header.Id && m.IsPaid);
                if (detail.Any())
                {
                    header.PawnDate = detail.Max(m => m.ToDate).AddDays(1);
                    songaydadong = detail.Sum(m => m.TotalDay) ?? 0; // songayvay - songaydadong
                    switch (header.InterestRateType)
                    {
                        case (int)MethodTypeEnum.PercentPerMonthDay:
                            header.PawnDate = detail.Max(m => m.ToDate);
                            valueMethod = 30;
                            break;
                        case (int)MethodTypeEnum.PercentPerMonth30:
                            valueMethod = 30;
                            break;
                        case (int)MethodTypeEnum.PercentPerWeek:
                        case (int)MethodTypeEnum.MoneyPerWeek:
                            valueMethod = 7;
                            break;
                        default:
                            valueMethod = 1;
                            break;
                    }
                }
                var songayconlai = (header.PawnDateNumber * valueMethod) - songaydadong;
                ConvertMethodOptimize(header, songayconlai ?? 0, valueMethod);
                message.Type = MessageTypeEnum.Success;
                message.Message = $"Thanh toán từ ngày {line.FromDate:dd-MM-yyyy} đến ngày {line.ToDate:dd-MM-yyyy} thành công!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> AddBHContract ", ex);
            }
            return message;

        }

        public MessageModels UpdateHopDong(PawnContractModels header)
        {
            var message = new MessageModels
            {
                Message = "Cập nhật hợp đồng thất bại!",
                Type = MessageTypeEnum.Error
            };
            try
            {
                var pawn = _unitOfWork.PawnContractRepository.Find(m => m.Id == header.Id);
                HistoryModels historyClose = new HistoryModels
                {
                    ContractID = header.Id,
                    Content = "Sửa Hợp Đồng",
                    StoreId = RDAuthorize.Store.Id,
                    TypeHistory = (int)DocumentTypeEnum.VayLai,
                    DebitMoney = (decimal)header.TotalMoney,
                    HavingMoney = (decimal)header.TotalMoney,
                    CreatedBy = RDAuthorize.Username,
                    CreatedDate = DateTime.Now
                };
                _historyServices.AddHistoryMessage(historyClose);
                _cashBookServices.AddCashBook(new CashBookModals
                {
                    CreatedDate = DateTime.Now,
                    CreatedUser = RDAuthorize.Username,
                    CreditAccount = (decimal)header.TotalMoney,
                    DebitAccount = (decimal)header.TotalMoney,
                    Note = "Sửa Hợp Đồng",
                    Customer = header.CustomerName,
                    DocumentDate = DateTime.Now,
                    DocumentType = (int)DocumentTypeEnum.VayLai,
                    VoucherType = (int)VocherTypeEnum.None,
                    StoreId = RDAuthorize.Store.Id,
                    IsActive = true,
                    IsDeleted = false,
                    ContractId = historyClose.ContractID
                });


                //if (header.MoneyIntroduce.HasValue && header.MoneyIntroduce > 0)
                //    _cashBookServices.AddCashBook(new CashBookModals
                //    {
                //        CreatedDate = DateTime.Now,
                //        CreatedUser = RDAuthorize.Username,
                //        CreditAccount = pawn.MoneyIntroduce ?? 0,
                //        DebitAccount = header.MoneyIntroduce ?? 0,
                //        Note = "Tiền giới thiệu",
                //        DocumentDate = DateTime.Now,
                //        DocumentType = (int)DocumentTypeEnum.VayLai,
                //        VoucherType = (int)VocherTypeEnum.PhieuChi,
                //        StoreId = RDAuthorize.Store.Id,
                //        IsActive = true,
                //        IsDeleted = false,
                //    });

                //if (header.MoneyServices.HasValue && header.MoneyServices > 0)
                //    _cashBookServices.AddCashBook(new CashBookModals
                //    {
                //        CreatedDate = DateTime.Now,
                //        CreatedUser = RDAuthorize.Username,
                //        CreditAccount = header.MoneyServices ?? 0,
                //        DebitAccount = pawn.MoneyServices ?? 0,
                //        Note = "Tiền dịch vụ",
                //        DocumentDate = DateTime.Now,
                //        DocumentType = (int)DocumentTypeEnum.VayLai,
                //        VoucherType = (int)VocherTypeEnum.PhieuThu,
                //        StoreId = RDAuthorize.Store.Id,
                //        IsActive = true,
                //        IsDeleted = false,
                //    });
                //  if (header.IsBefore != pawn.IsBefore ||
                //header.InterestRate != pawn.InterestRate ||
                //header.InterestRateOption != pawn.InterestRateOption ||
                //header.InterestRateType != pawn.InterestRateType ||
                //header.PawnDateNumber != pawn.PawnDateNumber ||
                //header.PawnDate != pawn.PawnDate ||
                //header.TotalMoney != pawn.TotalMoney)
                //  {
                DeletePawnPay(header.Id);
                // Khởi tạo các giá trị mới để add lại kỳ mới
                //*************gán giá trị mới**********
                var detail = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == header.Id && m.IsPaid);
                if (detail.Any())
                {
                    header.PawnDate = detail.Max(m => m.ToDate).AddDays(1);
                    var songaydadong = detail.Sum(m => m.TotalDay) ?? 0; // songayvay - songaydadong
                    switch (header.InterestRateType)
                    {
                        case (int)MethodTypeEnum.PercentPerMonthDay:
                            header.PawnDate = detail.Max(m => m.ToDate);
                            songaydadong = songaydadong / 30;
                            break;
                        case (int)MethodTypeEnum.PercentPerMonth30:
                            songaydadong = songaydadong / 30;
                            break;
                        case (int)MethodTypeEnum.PercentPerWeek:
                        case (int)MethodTypeEnum.MoneyPerWeek:
                            songaydadong = songaydadong / 7;
                            break;
                        default:
                            break;
                    }
                    header.PawnDateNumber -= songaydadong;
                }
                ConvertMethod(header);
                //}
                message.Type = MessageTypeEnum.Success;
                message.Message = "Cập nhật hợp đồng thành công!";

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> UpdateHopDong ", ex);
            }
            return message;
        }

        public MessageModels TraBotGoc(PawnContractModels header)
        {
            header.PawnDate = header.PawnDatePost;
            header.DocumentDate = header.DocumentDatePost;
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = header.IsNotification ? $"Vay thêm góc thất bại!" : $"Trả bớt góc thất bại!"
            };
            try
            {
                DeletePawnPay(header.Id);
                // Khởi tạo các giá trị mới để add lại kỳ mới
                //*************gán giá trị mới**********
                var detail = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == header.Id && m.IsPaid);
                if (detail.Any())
                {
                    header.PawnDate = detail.Max(m => m.ToDate).AddDays(1);
                    var songaydadong = detail.Sum(m => m.TotalDay) ?? 0; // songayvay - songaydadong
                    switch (header.InterestRateType)
                    {
                        case (int)MethodTypeEnum.PercentPerMonthDay:
                            header.PawnDate = detail.Max(m => m.ToDate);
                            songaydadong = songaydadong / 30;
                            break;
                        case (int)MethodTypeEnum.PercentPerMonth30:
                            songaydadong = songaydadong / 30;
                            break;
                        case (int)MethodTypeEnum.PercentPerWeek:
                        case (int)MethodTypeEnum.MoneyPerWeek:
                            songaydadong = songaydadong / 7;
                            break;
                    }
                    header.PawnDateNumber -= songaydadong;
                }
                var debt = _unitOfWork.CapitalLoanRepository.Filter(m => m.CapitalId == header.Id
                                                                         && m.DocumentType == (int)DocumentTypeEnum.VayLai).ToList();
                double vaythem = 0; double tragoc = 0;
                if (debt.Any())
                {
                    vaythem = (double)debt.Where(m => m.IsLoan).Sum(m => m.MoneyNumber);
                    tragoc = (double)debt.Where(m => !m.IsLoan).Sum(m => m.MoneyNumber);
                }

                header.TienVayThem = vaythem - tragoc;
                ConvertMethod(header);
                message.Type = MessageTypeEnum.Success;
                message.Message = "Trả bớt góc thành công!";

            }
            catch (Exception ex)
            {
                PawnLog.Error("BathoServices --> AddBHContract ", ex);
            }
            return message;
        }

        public MessageModels DongHopDong(int contractId, double totalMoney)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Đóng hợp đồng thất bại!"
            };
            try
            {
                // update contract
                var contract = _unitOfWork.PawnContractRepository.FindById(contractId);
                if (contract.IsCloseContract)
                {
                    message.Message = $"Hợp đồng này đã đóng!. Bạn phải mở lại hợp đồng để có thể cập nhật hợp đồng!";
                    return message;
                }
                contract.ClosedDate = DateTime.Now;
                contract.ClosedUser = RDAuthorize.Username;
                contract.IsCloseContract = true;
                _unitOfWork.PawnContractRepository.Update(contract);
                string customerName = _customerServices.GetCustomerByStoreId(contract.StoreId).Where(m => m.Id == contract.CustomerId).Select(m => m.Fullname).FirstOrDefault();
                // create phieu thu
                HistoryModels historyClose = new HistoryModels
                {
                    ContractID = contractId,
                    Content = "Đóng hợp đồng",
                    StoreId = RDAuthorize.Store.Id,
                    TypeHistory = (int)DocumentTypeEnum.VayLai,
                    DebitMoney = 0,
                    HavingMoney = (decimal)totalMoney,
                    CreatedBy = RDAuthorize.Username,
                    CreatedDate = DateTime.Now
                };
                _historyServices.AddHistoryMessage(historyClose);

                _cashBookServices.AddCashBook(new CashBookModals
                {
                    CreatedDate = DateTime.Now,
                    CreatedUser = RDAuthorize.Username,
                    CreditAccount = (decimal)totalMoney,
                    DebitAccount = 0,
                    Note = "Đóng hợp đồng",
                    NoteId = (int)NotesEnum.DongHopDongVaiLai,
                    Customer = customerName,
                    DocumentDate = DateTime.Now,
                    DocumentType = (int)DocumentTypeEnum.VayLai,
                    VoucherType = (int)VocherTypeEnum.PhieuThu,
                    StoreId = RDAuthorize.Store.Id,
                    IsActive = true,
                    IsDeleted = false,
                    ContractId = historyClose.ContractID
                });

                var result = _unitOfWork.SaveChanges();
                message.Type = MessageTypeEnum.Success;
                message.Message = "Đóng hợp đồng thành công!";

            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> Đóng hợp đồng ", ex);
            }
            return message;
        }

        public MessageModels GiaHanThem(PawnContractModels header, int day)
        {
            header.PawnDate = header.PawnDatePost;
            header.DocumentDate = header.DocumentDatePost;
            header.PawnDateNumber += day;
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Gia hạn hợp đồng thất bại!"
            };
            try
            {
                // update contract
                var contract = _unitOfWork.PawnContractRepository.Find(m => m.Id == header.Id);
                if (contract.IsCloseContract)
                {
                    message.Message = $"Hợp đồng này đã đóng!. Bạn phải mở lại hợp đồng để có thể cập nhật hợp đồng!";
                    return message;
                }
                contract.PawnDateNumber += day;
                contract.Code = contract.DocumentName;
                _unitOfWork.PawnContractRepository.Update(contract);
                var result = _unitOfWork.SaveChanges();
                if (result > 0)
                {
                    // create phieu thu
                    HistoryModels historyClose = new HistoryModels
                    {
                        ContractID = header.Id,
                        Content = "Gia hạn",
                        StoreId = RDAuthorize.Store.Id,
                        TypeHistory = (int)DocumentTypeEnum.VayLai,
                        DebitMoney = 0,
                        HavingMoney = 0,
                        CreatedBy = RDAuthorize.Username,
                        CreatedDate = DateTime.Now
                    };
                    _historyServices.AddHistoryMessage(historyClose);
                    message.Type = MessageTypeEnum.Success;
                    message.Message = "Gia hạn hợp đồng thành công!";
                    // xử lý
                    // Đã tính luôn nợ góc và lưu vào bảng.
                    DeletePawnPay(header.Id);
                    // Khởi tạo các giá trị mới để add lại kỳ mới
                    //*************gán giá trị mới**********
                    var detail = _unitOfWork.PawnPayRepository.Filter(m => m.ContractId == header.Id && m.IsPaid);
                    if (detail.Any())
                    {
                        header.PawnDate = detail.Max(m => m.ToDate).AddDays(1);
                        var songaydadong = detail.Sum(m => m.TotalDay) ?? 0; // songayvay - songaydadong
                        switch (header.InterestRateType)
                        {
                            case (int)MethodTypeEnum.PercentPerMonthDay:
                                header.PawnDate = detail.Max(m => m.ToDate);
                                songaydadong = songaydadong / 30;
                                break;
                            case (int)MethodTypeEnum.PercentPerMonth30:
                                songaydadong = songaydadong / 30;
                                break;
                            case (int)MethodTypeEnum.PercentPerWeek:
                            case (int)MethodTypeEnum.MoneyPerWeek:
                                songaydadong = songaydadong / 7;
                                break;
                        }
                        header.PawnDateNumber -= songaydadong;
                    }
                    ConvertMethod(header);
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> Gia hạn hợp đồng ", ex);
            }
            return message;

        }

        public List<ExtentionContractModels> LoadListExtentionContract(List<ExtentionContractModels> lstData, int method)
        {
            MethodTypeEnum capitalMethod = (MethodTypeEnum)method;
            var listExtentionContract = new List<ExtentionContractModels>();
            foreach (var item in lstData)
            {
                var fromDate = item.ToDateContract.Value;
                var objExtention = new ExtentionContractModels
                {
                    ContractID = item.ContractID,
                    Id = item.Id,
                    Note = item.Note,
                    FromDate = fromDate
                };

                switch (capitalMethod)
                {
                    case MethodTypeEnum.MoneyPerDay:
                        objExtention.AddTime = item.AddTime;
                        break;
                    case MethodTypeEnum.PercentPerMonth30:
                        objExtention.AddTime = item.AddTime * 30;
                        break;
                    case MethodTypeEnum.PercentPerMonthDay:
                        var dtToDate = fromDate.AddMonths(item.AddTime);
                        var addTime = (int)(dtToDate - fromDate).TotalDays;
                        objExtention.AddTime = addTime;
                        break;
                    case MethodTypeEnum.PercentPerWeek:
                    case MethodTypeEnum.MoneyPerWeek:
                        objExtention.AddTime = item.AddTime * 7;
                        break;
                    default:
                        objExtention.AddTime = 0;
                        break;
                }
                objExtention.ToDate = fromDate.AddDays(objExtention.AddTime - 1);
                listExtentionContract.Add(objExtention);
            }
            return listExtentionContract;
        }

        public MessageModels DeletePawn(int idPawn)
        {
            var message = new MessageModels
            {
                Message = "Xóa hợp đồng vay lãi thất bại",
                Type = MessageTypeEnum.Error
            };

            try
            {
                var result = -1;
                var pawn = _unitOfWork.PawnContractRepository.Find(s => s.Id == idPawn);
                if (pawn != null)
                {
                    pawn.IsDeleted = !pawn.IsDeleted;
                    pawn.DeletedDate = DateTime.Now;
                    pawn.DeletedUser = RDAuthorize.Username;
                    result = _unitOfWork.SaveChanges();
                }
                if (result > 0)
                {
                    message.Type = MessageTypeEnum.Success;
                    _cashBookServices.AddCashBook(new CashBookModals
                    {
                        CreatedDate = DateTime.Now,
                        CreatedUser = RDAuthorize.Username,
                        CreditAccount = pawn.IsDeleted == true ? (decimal)pawn.TotalMoney : 0,
                        DebitAccount = pawn.IsDeleted == false ? (decimal)pawn.TotalMoney : 0,
                        Note = "Xóa hợp đồng vay lãi",
                        NoteId = (int)NotesEnum.XoaHDVayLai,
                        Customer = _unitOfWork.CustomerRepository.Find(s => s.Id == pawn.CustomerId)?.Fullname,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.VayLai,
                        VoucherType = pawn.IsDeleted ? (int)VocherTypeEnum.PhieuThu : (int)VocherTypeEnum.PhieuChi,
                        StoreId = RDAuthorize.Store.Id,
                        IsActive = true,
                        IsDeleted = false,
                        ContractId = idPawn
                    });
                }
                message.Message = $"{(pawn.IsDeleted ? "Xóa" : "Khôi phục")} hợp đồng vay lãi thành công";
            }
            catch (Exception)
            {

                throw;
            }

            return message;
        }

        public List<PawnContractModels> LoadDataCustomerPaidTomorrow(InterestPaid addDay)
        {
            try
            {
                var _params = new object[] { RDAuthorize.Store.Id, (int)addDay };
                var data = _unitOfWork.ExecStoreProdure<PawnContractModels>("SP_LOAD_CUSTOMER_PAID_TOMORROW_VL {0}, {1}", _params).ToList();
                for (int i = 0; i < data.Count; i++)
                {
                    data[i].TienLaiMotNgay = MoneyByDay(data[i], 0);
                    data[i].TotalMoney = data[i].TienLaiMotNgay * data[i].NumOfDays;
                }
                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> LoadDataCustomerPaidTomorrow ", ex);
                return new List<PawnContractModels>();
            }
        }

        public string GetMaxVLContract()
        {
            string maxvalue = "1";
            var code = _unitOfWork.PawnContractRepository.OrderByDescending(s => s.Id).FirstOrDefault();
            if (code != null)
                maxvalue = RDAuthorize.Store.Id + "" + ((code?.Id ?? 0) + 1);
            return maxvalue;
        }

        public MemoryStream ExportExcel()
        {
            MemoryStream stream;
            var excelPackage = new ExcelPackages("Danh sách hợp đồng vay lãi");
            try
            {
                var _params = new object[] { RDAuthorize.Store.Id };
                var data = _unitOfWork.ExecStoreProdure<PawnExportExcel>("SP_VAYLAI_EXPORT_EXCEL {0}", _params).ToList();
                var dt = new DataTable();
                for (int i = 0; i < 11; i++)
                    dt.Columns.Add(i + "", typeof(string));
                dt.Rows.Add("Mã HĐ", "Tên KH", "SĐT", "Tiền vay", "Lãi", "Ngày vay", "Ngày hết hạn"
                    , "Ghi Chú Vay Lãi", "Đã đóng lãi đến", "Tiền lãi đã đóng", "Ngày đóng lãi tiếp theo");
                foreach (var item in data)
                {
                    dt.Rows.Add(item.Code, item.Fullname, item.Phone, item.TotalMoney.ToPrice(), item.InterestRate + item.InterestRateString,
                        item.PawnDate, item.ToDate, item.Note, item.DatePaid, item.MoneyPaid.ToPrice(), item.NextDate);
                }
                stream = excelPackage.ExportToExcel(dt);
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> ExportExcel ", ex);
                stream = null;
            }
            return stream;
        }

        public MessageModels ConvertStore(int vlId, int storeId)
        {
            var rs = -1;
            var cusid = -1;
            int docType = (int)DocumentTypeEnum.VayLai;
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Chuyển cửa hàng thất bại"
            };

            try
            {
                var vl = _unitOfWork.PawnContractRepository.Find(s => s.Id == vlId);
                if (vl != null)
                {
                    //if (vl.CreatedDate.Date < Convert.ToDateTime("10/29/2018").Date)
                    //{
                    //    message.Message = "Chỉ được chuyển những hợp đồng được tạo sau ngày 28-10-2018";
                    //    return message;
                    //}
                    var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == vl.CustomerId);
                    customer.StoreId = storeId;
                    _unitOfWork.CustomerRepository.Insert(customer);
                    rs = _unitOfWork.SaveChanges();
                    cusid = customer.Id;
                }

                if (rs > 0)
                {
                    vl.StoreId = storeId;
                    vl.CustomerId = cusid;

                    var cb = _unitOfWork.CashBookRepository.Filter(s => s.ContractId == vlId && s.DocumentType == docType);
                    if (cb != null)
                    {
                        foreach (var item in cb)
                        {
                            item.StoreId = storeId;
                            _unitOfWork.CashBookRepository.Update(item);
                        }
                    }

                    var history = _unitOfWork.HistoryRepository.Filter(s => s.ContractID == vlId && s.TypeHistory == docType);
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
                PawnLog.Error("PawnServices --> ConvertStore ", ex);
            }

            return message;
        }


        public MessageModels UpdateBadDebt(int idVayLai, bool isBadDebt)
        {
            var mess = new MessageModels
            {
                Message = $"Đã chuyển hợp đồng thành {(isBadDebt == false ? "bình thường" : "nợ xấu")} thất bại",
                Type = MessageTypeEnum.Error
            };
            try
            {
                var rs = -1;
                var vl = _unitOfWork.PawnContractRepository.Find(s => s.Id == idVayLai);
                if (vl != null)
                {
                    vl.IsBadDebt = isBadDebt;
                    vl.UpdatedDate = DateTime.Now;
                    vl.UpdatedUser = RDAuthorize.Username;
                    rs = _unitOfWork.SaveChanges();
                }
                if (rs > 0)
                {
                    mess.Type = MessageTypeEnum.Success;
                    mess.Message = $"Đã chuyển hợp đồng thành {(isBadDebt == false ? "bình thường" : "nợ xấu")}";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> UpdateBadDebt ", ex);
            }
            return mess;
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
                var vl = _unitOfWork.PawnContractRepository.Find(s => s.Id == contractId);
                decimal moneyIntro = 0;
                decimal moneySer = 0;
                if (vl != null)
                {
                    moneyIntro = vl.MoneyIntroduce ?? 0;
                    moneySer = vl.MoneyServices ?? 0;
                    vl.MoneyIntroduce = moneyIntroduce ?? 0;
                    vl.MoneyServices = moneyService ?? 0;
                    vl.UpdatedDate = DateTime.Now;
                    vl.UpdatedUser = RDAuthorize.Username;
                    rs = _unitOfWork.SaveChanges();
                }
                if (rs > 0)
                {
                    mess.Type = MessageTypeEnum.Success;
                    mess.Message = "Cập nhật tiền dịch vụ và tiền giới thiệu thành công";
                    var customer = _unitOfWork.CustomerRepository.Find(s => s.Id == vl.CustomerId)?.Fullname;
                    if (moneyIntroduce.HasValue && moneyIntroduce > 0)
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CreditAccount = moneyIntro,
                            DebitAccount = vl.MoneyIntroduce ?? 0,
                            Note = "Cập nhật tiền giới thiệu HD " + vl.Code,
                            NoteId = (int)NotesEnum.TienGioiThieuHopDongVaiLai,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.VayLai,
                            VoucherType = (int)VocherTypeEnum.PhieuChi,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            Customer = customer,
                            IsDeleted = false,
                            ContractId = vl.Id
                        });

                    if (moneyService.HasValue && moneyService > 0)
                        _cashBookServices.AddCashBook(new CashBookModals
                        {
                            CreatedDate = DateTime.Now,
                            CreatedUser = RDAuthorize.Username,
                            CreditAccount = vl.MoneyServices ?? 0,
                            DebitAccount = moneySer,
                            Customer = customer,
                            Note = "Cập nhật tiền dịch vụ HD " + vl.Code,
                            NoteId = (int)NotesEnum.TienDichVuHopDongVayLai,
                            DocumentDate = DateTime.Now,
                            DocumentType = (int)DocumentTypeEnum.VayLai,
                            VoucherType = (int)VocherTypeEnum.PhieuThu,
                            StoreId = RDAuthorize.Store.Id,
                            IsActive = true,
                            IsDeleted = false,
                            ContractId = vl.Id
                        });
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnServices --> UpdateMoneySI ", ex);
            }

            return mess;
        }
    }
}
