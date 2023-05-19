using AutoMapper;
using Pawn.Authorize;
using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using Pawn.Libraries;
using Pawn.Logger;
using Pawn.ViewModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawn.Services
{
    public class CapitalServices : ICapitalServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICapitalLoanServices _capitalLoan;
        private readonly IHistoryServices _history;
        private readonly IExtentionContractServices _extentionContract;
        private readonly ICashBookServices _cashBookServices;

        public CapitalServices(IUnitOfWork unitOfWork, IMapper mapper, ICapitalLoanServices capitalLoan,
                               IHistoryServices history, IExtentionContractServices extentionContract,
                               ICashBookServices cashBookServices)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _capitalLoan = capitalLoan;
            _history = history;
            _extentionContract = extentionContract;
            _cashBookServices = cashBookServices;
        }

        public List<CapitalModels> LoadDataCapital(string strKeyword, DateTime? dtToDate, DateTime? dtFromDate, int intStatusContract, int intPageSize, int intPageIndex)
        {
            try
            {
                var _params = new object[] { RDAuthorize.Store.Id, strKeyword, dtToDate, dtFromDate, intStatusContract, intPageSize, intPageIndex - 1 };
                var data = _unitOfWork.ExecStoreProdure<CapitalModels>("SP_CAPITAL_SRH {0}, {1}, {2}, {3}, {4}, {5}, {6}", _params).ToList();
                for (int i = 0; i < data.Count; i++)
                {
                    data[i].LoanDate = GetLoanDate(data[i]);

                    if (data[i].LoanDate.Date == DateTime.Now.Date)
                        data[i].StatusType = (int)StatusTypeCapital.DenNgay;
                    else if (data[i].LoanDate.Date < DateTime.Now.Date)
                        data[i].StatusType = (int)StatusTypeCapital.NoLai;

                    if (data[i].Method != 14)
                        data[i].RateTypeName = Calculator.GetNameOfRateType(data[i].RateType ?? -1);
                }

                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalServices --> LoadDataCapital ", ex);
                return new List<CapitalModels>();
            }
        }

        private DateTime GetLoanDate(CapitalModels objCapital)
        {
            var toDate = Calculator.ReturnFromToDate(objCapital.Method, objCapital.DocumentDate, (objCapital.BorrowNumber ?? 0));
            var capitalPayDays = _unitOfWork.CapitalPayDayRepository.Filter(s => s.CapitalId == objCapital.Id).ToList();
            var fromDate = capitalPayDays.Count > 0 ? capitalPayDays.LastOrDefault().ToDate.AddDays(1) : objCapital.DocumentDate;
            var day = Calculator.GetDayToAdd((objCapital.BorrowPeriod ?? 0), objCapital.Method, fromDate);

            var fromDateLoam = fromDate;
            var toDateLoan = fromDate.AddDays(day - 1);
            if (toDateLoan > toDate.ToDateTime(Constants.DateFormat)) toDateLoan = toDate.ToDateTime(Constants.DateFormat);
            return objCapital.IsBeforeReceipt ? fromDateLoam : toDateLoan;
        }

        public MessageModels AddCapital(CapitalModels objCapital)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"{(objCapital.Id < 1 ? "Thêm mới" : "Cập nhật ")} nguồn vốn thất bại!"
            };
            try
            {
                var result = -1;
                Tb_Capital capital;
                var history = new HistoryModels
                {
                    Content = message.Message.Replace(" thất bại!", ""),
                };

                if (objCapital.IsSystem == false)
                {
                    var customer = new Tb_Customer()
                    {
                        CreatedUser = RDAuthorize.Username,
                        Fullname = objCapital.CustomerName,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Phone = objCapital.Phone,
                        StoreId = objCapital.StoreId
                    };
                    _unitOfWork.CustomerRepository.Insert(customer);
                    result = _unitOfWork.SaveChanges();
                    objCapital.CustomerId = customer.Id;
                }

                if (objCapital.Id < 1)
                {
                    if (result > 0 || objCapital.IsSystem)
                    {
                        objCapital.Status = objCapital.StoreId;
                        objCapital.IsActive = true;
                        objCapital.DocumentType = (int)DocumentTypeEnum.NguonVon;
                        objCapital.StoreId = objCapital.StoreId;
                        capital = _mapper.Map<CapitalModels, Tb_Capital>(objCapital);
                        _unitOfWork.CapitalRepository.Insert(capital);
                        history.HavingMoney = objCapital.MoneyNumber;
                    }
                    else return message;
                }
                else
                {
                    capital = _unitOfWork.CapitalRepository.Find(s => s.Id == objCapital.Id);
                    if (capital != null)
                    {
                        history.DebitMoney = capital.MoneyNumber;
                        history.HavingMoney = objCapital.MoneyNumber;

                        capital.CustomerId = objCapital.CustomerId;
                        var cusname = _unitOfWork.CustomerRepository.Find(s => s.Id == objCapital.CustomerId)?.Fullname ?? "";
                        capital.CustomerName = cusname;
                        capital.Phone = objCapital.Phone;
                        capital.MoneyNumber = objCapital.MoneyNumber;
                        capital.DocumentDate = objCapital.DocumentDate;
                        capital.Method = objCapital.Method;
                        capital.Note = objCapital.Note;
                        capital.IsSystem = objCapital.IsSystem;
                        capital.IsBeforeReceipt = objCapital.IsBeforeReceipt;
                        capital.InterestRate = objCapital.InterestRate;
                        capital.RateType = objCapital.RateType;
                        capital.BorrowNumber = objCapital.BorrowNumber;
                        capital.BorrowPeriod = objCapital.BorrowPeriod;
                    }
                }
                result = _unitOfWork.SaveChanges();
                if (result > 0)
                {
                    history.ContractID = capital.Id;
                    history.TypeHistory = (int)DocumentTypeEnum.NguonVon;
                    history.StoreId = objCapital.StoreId;
                    _history.AddHistory(history);

                    var cashBook = new CashBookModals
                    {
                        StoreId = objCapital.StoreId,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.NguonVon,
                        CreditAccount = history.HavingMoney ?? 0,
                        DebitAccount = history.DebitMoney ?? 0,
                        VoucherType = 1,
                        Customer = capital.CustomerName,
                        Note = history.Content,
                        ContractId = history.ContractID
                    };

                    _cashBookServices.AddCashBook(cashBook);
                }
                message.Type = result > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = $"{(objCapital.Id < 1 ? "Thêm mới" : "Cập nhật ")} nguồn vốn thành công!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnAccountServices --> AddAccount ", ex);
            }

            return message;
        }

        public CapitalModels LoadDetailCapital(int intCapitalId)
        {
            try
            {
                var capital = _unitOfWork.CapitalRepository.Filter(s => s.Id == intCapitalId)
                                                           .Join(_unitOfWork.CustomerRepository.Filter(s => s.StoreId == RDAuthorize.Store.Id),
                                                                cp => cp.CustomerId,
                                                                ct => ct.Id,
                                                                (cp, ct) => new { cp, ct.Fullname, ct.Phone })
                                                           .Select(s => new CapitalModels
                                                           {
                                                               DocumentDate = s.cp.DocumentDate,
                                                               CustomerName = s.Fullname,
                                                               Phone = s.Phone,
                                                               MoneyNumber = s.cp.MoneyNumber,
                                                               Method = s.cp.Method,
                                                               InterestRate = s.cp.InterestRate,
                                                               BorrowNumber = s.cp.BorrowNumber,
                                                               BorrowPeriod = s.cp.BorrowPeriod,
                                                               IsBeforeReceipt = s.cp.IsBeforeReceipt,
                                                               IsSystem = true,
                                                               Note = s.cp.Note,
                                                               RateType = s.cp.RateType,
                                                               Id = s.cp.Id,
                                                               CustomerId = s.cp.CustomerId
                                                           }).FirstOrDefault() ?? new CapitalModels();
                if (capital.Id > 0)
                {
                    capital.DocumentDateString = capital.DocumentDate.ToString(Constants.DateFormat);
                    capital.IsPaid = _unitOfWork.CapitalPayDayRepository.HasRows(s => s.CapitalId == capital.Id);
                }
                return capital;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalServices --> LoadDetailCapital ", ex);
                return new CapitalModels();
            }
        }

        public CapitalDetailModels LoadCapitalDetail(int intCapitalId)
        {
            try
            {
                var docType = (int)DocumentTypeEnum.NguonVon;
                var data = _unitOfWork.CapitalRepository.Filter(s => s.Id == intCapitalId && s.DocumentType == docType)
                                      .Join(_unitOfWork.StatusRepository.Filter(s => s.Type == 3),
                                                cp => cp.Method,
                                                st => st.Id,
                                                (cp, st) => new { cp, st.StatusName })
                                      .Join(_unitOfWork.CustomerRepository.Filter(s => s.StoreId == RDAuthorize.Store.Id),
                                                                 cp => cp.cp.CustomerId,
                                                                 ct => ct.Id,
                                                                 (cp, ct) => new { cp.cp, ct.Fullname, cp.StatusName })
                                      .Select(s => new CapitalDetailModels
                                      {
                                          TotalMoney = s.cp.MoneyNumber,
                                          TotalMoneySys = s.cp.MoneyNumber,
                                          InterestRate = s.cp.InterestRate,
                                          DocumentDate = s.cp.DocumentDate,
                                          Method = s.cp.Method,
                                          RateType = s.cp.RateType,
                                          BorrowNumber = s.cp.BorrowNumber,
                                          BorrowPeriod = s.cp.BorrowPeriod,
                                          Id = s.cp.Id,
                                          CustomerName = s.Fullname,
                                          IsBeforeReceipt = s.cp.IsBeforeReceipt
                                      }).FirstOrDefault() ?? new CapitalDetailModels();


                if (data.Id > 0)
                {
                    var capitalLoan = _unitOfWork.CapitalLoanRepository.Filter(s => s.CapitalId == intCapitalId && s.DocumentType == docType).ToList();
                    var extentionData = _extentionContract.LoadDataExtentionContract(data.Id, DocumentTypeEnum.NguonVon);

                    var extention = extentionData.Sum(s => s.AddTime);
                    string toDate = Calculator.ReturnFromToDate(data.Method, data.DocumentDate, (data.BorrowNumber ?? 0));
                    data.BorrowNumber += extention;
                    decimal total = 0;
                    if (capitalLoan.Any())
                    {
                        var havingMoney = capitalLoan.Where(s => s.IsLoan).ToList().Sum(s => s.MoneyNumber);
                        var debitMoney = capitalLoan.Where(s => s.IsLoan == false).ToList();
                        data.CapitalLoanWithdrawn = GetTotalMoneyCapitalLoan(debitMoney, data.DocumentDate);
                        total = havingMoney - debitMoney.Sum(s => s.MoneyNumber);
                        data.ListCapitalLoan = _mapper.Map<List<Tb_Capital_Loan>, List<CapitalLoanModels>>(capitalLoan);
                    }

                    if (data.Method > 0)
                    {
                        data.MethodName = Calculator.GetNameOfRateType(data.RateType ?? -1);
                        data.FromDate = data.DocumentDate.ToString(Constants.DateFormat);
                        data.ToDate = Calculator.ReturnFromToDate(data.Method, data.DocumentDate, (data.BorrowNumber ?? 0));
                    }
                    data.ListExtentionContract = LoadListExtentionContract(extentionData, toDate.ToDateTime(Constants.DateFormat), data.Method);

                    if (data.Method > (int)CapitalMethodEnum.VonDauTu)
                    {
                        // get sum money of capital
                        var capitalPayDays = _unitOfWork.CapitalPayDayRepository.Filter(s => s.CapitalId == intCapitalId).ToList();
                        data.TotalMoneyOrther = capitalPayDays.Sum(s => s.MoneyOrther) ?? 0;
                        var mapData = _mapper.Map<List<Tb_Capital_PayDay>, List<CapitalPayDayModels>>(capitalPayDays);
                        data.MoneyPaid = mapData.Sum(s => s.MoneyNumber) + (mapData.Sum(s => s.MoneyOrther) ?? 0);
                        var dateCapDay = capitalPayDays.OrderBy(s => s.FromDate).LastOrDefault()?.ToDate.AddDays(1) ?? data.DocumentDate;
                        data.CapitalPayDayDate = dateCapDay.ToString(Constants.DateFormat);

                        /*
                         * mapData: Danh sách số kì đã đóng tiền
                         * capitalLoan: danh sách số tiền rút vốn/vay thêm vốn
                         * data: nguồn vốn
                        */
                        data.ListCapitalPayDay = LoadCapitalPayday(mapData, capitalLoan.ToList(), data);
                        data.TotalInterest = data.ListCapitalPayDay.Sum(s => s.MoneyNumber) + (data.ListCapitalPayDay.Sum(s => s.MoneyOrther) ?? 0);
                        if (data.ListCapitalPayDay.Any())
                        {
                            var objData = data.ListCapitalPayDay.OrderBy(s => s.FromDate).FirstOrDefault(s => s.IsPaid == false);
                            if (objData != null)
                            {
                                data.LoanDate = data.IsBeforeReceipt ? objData.FromDate : objData.ToDate;
                                data.LastLoanDate = objData.ToDate;
                                if (data.LoanDate.Date < DateTime.Now.Date)
                                    data.StatusType = (int)StatusTypeCapital.NoLai;
                            }
                        }
                    }

                    data.TotalMoney = data.TotalMoney + total;
                }

                return data;
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalServices --> LoadCapitalDetail ", ex);
                return new CapitalDetailModels();
            }
        }

        public List<CapitalPayDayModels> LoadCapitalPayday(List<CapitalPayDayModels> lstData, List<Tb_Capital_Loan> lstDataCapitalLoan, CapitalDetailModels capital)
        {
            var listDate = new List<CapitalPayDayModels>();
            decimal total = 0;
            foreach (var item in lstData)
            {
                item.NumberOfDay = (int)(item.ToDate - item.FromDate).TotalDays + 1;
                item.IsPaid = true;
                total += item.MoneyNumber;
                listDate.Add(item);
            }
            //Nếu đã có 1 kì đóng lãi thì ngày ngày ToDate + 1 ngày, ngược lại lấy ngày vay vốn
            var fromDate = lstData.Count > 0 ? lstData.LastOrDefault().ToDate.AddDays(1) : capital.DocumentDate;

            // nếu là vay lãi tháng định kì và đã có đóng lãi 1 kì bất kì thì kì tiếp theo chưa đóng lãi ngày bắt đầu = todate kì đóng gần nhất
            if (lstData.Count > 0 && (CapitalMethodEnum)capital.Method == CapitalMethodEnum.LaiThangDinhKy)
                fromDate = fromDate.AddDays(-1);
            var endDate = capital.ToDate.ToDateTime(Constants.DateFormat);
            int index = 0;
            while (index <= 10000)// tranh truong hop 1 ngay nao do khong nhay vao break =)))
            {
                // lấy số ngày + thêm theo loại (Method)
                var day = Calculator.GetDayToAdd((capital.BorrowPeriod ?? 0), capital.Method, fromDate);

                var objCapitalPayDay = new CapitalPayDayModels();
                objCapitalPayDay.CapitalId = capital.Id;
                objCapitalPayDay.FromDate = fromDate;
                // -1 vì tính cả ngày đầu kì vì trong c# khi adddays ko tính ngày đầu
                objCapitalPayDay.ToDate = fromDate.AddDays(day - 1);
                // + 1 vì tính cả ngày đầu kì
                objCapitalPayDay.NumberOfDay = (int)(objCapitalPayDay.ToDate - objCapitalPayDay.FromDate).TotalDays + 1;
                objCapitalPayDay.MoneyOrther = 0;
                // nếu là đóng lãi tháng định kì
                // Vì ngày được add đúng 1 tháng (ex: 25/09 - 25/10 ) nên ko tính tiền theo ngày được vì có thế 1 tháng có 29 hoặc 30 hoặc 31 ngày => tính theo tháng => numberofday = 1 tháng
                objCapitalPayDay.MoneyNumber = ((CapitalMethodEnum)capital.Method != CapitalMethodEnum.LaiThangDinhKy ? objCapitalPayDay.NumberOfDay : 1)
                                                // lấy số tiền trên 1 ngày
                                                * GetMoneyPerDay(capital, lstDataCapitalLoan, objCapitalPayDay.ToDate)
                                                // + thêm số tiền khi rút vốn hoặc vay lãi theo ngày
                                                + GetOrtherMoney(lstDataCapitalLoan, capital, objCapitalPayDay.FromDate, objCapitalPayDay.ToDate);
                objCapitalPayDay.IsPaid = false;
                // nếu todate mà lớn hơn hoặc = ngày cuối cùng đóng lãi của hợp đồng, thì gen lại data
                // lúc đó todate = enddate. (enddate là ngày đóng lãi cuối cùng của hợp đồng)
                if (objCapitalPayDay.ToDate >= endDate)
                {
                    objCapitalPayDay.ToDate = endDate;
                    objCapitalPayDay.NumberOfDay = (int)(objCapitalPayDay.ToDate - objCapitalPayDay.FromDate).TotalDays + 1;
                    if ((CapitalRateTypeEnum)capital.RateType == CapitalRateTypeEnum.PercentPerWeek)
                        // tính tiền theo tuần thì ko quan tâm nó rút vốn hay vay thêm vốn, cứ 1 tuần là 1 số tiền nhất định
                        // => số ngày còn lại sẽ = tổng số tiền lãi - số ngày tính lãi đã add vào list để
                        objCapitalPayDay.MoneyNumber = (((capital.TotalMoney * (capital.InterestRate ?? 1)) / 100) * (capital.BorrowNumber ?? 1)) - total;
                    else if ((CapitalRateTypeEnum)capital.RateType == CapitalRateTypeEnum.KPerWeek)
                        // tương tự cái trên
                        objCapitalPayDay.MoneyNumber = (((capital.BorrowNumber ?? 1) * (capital.InterestRate ?? 1)) * 1000) - total;
                    else
                        objCapitalPayDay.MoneyNumber =
                            ((CapitalMethodEnum)capital.Method != CapitalMethodEnum.LaiThangDinhKy ? objCapitalPayDay.NumberOfDay : 1)
                                    // lấy số tiền trên 1 ngày
                                    * GetMoneyPerDay(capital, lstDataCapitalLoan, objCapitalPayDay.ToDate)
                                    // + thêm số tiền khi rút vốn hoặc vay lãi theo ngày
                                    + GetOrtherMoney(lstDataCapitalLoan, capital, objCapitalPayDay.FromDate, objCapitalPayDay.ToDate);
                    listDate.Add(objCapitalPayDay);
                    break;
                }
                listDate.Add(objCapitalPayDay);
                var enumMethod = (CapitalMethodEnum)capital.Method;
                // nếu là vay lãi tháng định kì thì kì tiếp theo chưa đóng lãi ngày có bắt đầu = todate của kì đóng gần nhất
                fromDate = capital.Method != (int)CapitalMethodEnum.LaiThangDinhKy ? fromDate.AddDays(day) : fromDate.AddDays(day - 1);
                total += objCapitalPayDay.MoneyNumber;
                index++;
            }
            return listDate;
        }

        // tính số tiền lãi theo số tiền rút vốn theo từng kì. công thức: (ngày rút - ngày đầu kì) * số tiền/ngày của số tiền rút
        public decimal GetOrtherMoney(List<Tb_Capital_Loan> lstCapitalLoan, CapitalDetailModels capital, DateTime fromDate, DateTime toDate)
        {
            // đóng lãi theo tuần không tính tiền rút hoặc vay
            if ((CapitalRateTypeEnum)capital.RateType == CapitalRateTypeEnum.KPerWeek || (CapitalRateTypeEnum)capital.RateType == CapitalRateTypeEnum.PercentPerWeek)
                return 0;
            // lấy list ngày đóng tiền trong kì
            var lstData = lstCapitalLoan.Where(s => s.LoadDate >= fromDate && s.LoadDate <= toDate).ToList();

            decimal total = 0;
            for (int i = 0; i < lstData.Count(); i++)
            {
                var item = lstData[i];
                var cpt = new CapitalDetailModels
                {
                    TotalMoney = item.MoneyNumber,
                    RateType = capital.RateType,
                    InterestRate = capital.InterestRate,
                };
                cpt.TotalMoney = item.MoneyNumber;
                var moneyPerDay = GetMoneyPerDay(cpt); // lấy số tiền trên ngày
                if (item.IsLoan == false) // IsLoan = false là Rút vốn, = true là vay thêm vốn
                    total += (((int)(item.LoadDate - fromDate).TotalDays + 1) * moneyPerDay); // ngày rút - ngày đầu kì * với số tiền/ngày (+1 tính cả ngày đầu kì)
                else total -= (((int)(item.LoadDate - fromDate).TotalDays) * moneyPerDay); // ngày vay - ngày đầu kì * số tiền/ngày
            }
            return total;
        }

        // Tính ra số tiền/ngày
        public decimal GetMoneyPerDay(CapitalDetailModels capitalDetail, List<Tb_Capital_Loan> lstCapitalLoan = null, DateTime? toDate = null)
        {
            CapitalRateTypeEnum capitalRateType = (CapitalRateTypeEnum)capitalDetail.RateType;
            var total = capitalDetail.TotalMoney;
            // đóng lãi theo tuần không tính tiền rút hoặc vay
            if ((lstCapitalLoan != null && lstCapitalLoan.Count > 0) && toDate != null
                && (CapitalRateTypeEnum)capitalDetail.RateType != CapitalRateTypeEnum.KPerWeek && (CapitalRateTypeEnum)capitalDetail.RateType != CapitalRateTypeEnum.PercentPerWeek)
            {
                var money = lstCapitalLoan.Where(s => s.LoadDate <= toDate && s.IsLoan == false).ToList(); // rút vốn
                var moneyLoan = lstCapitalLoan.Where(s => s.LoadDate <= toDate && s.IsLoan == true).ToList(); // vay thêm tiền

                // tổng tiền =  tổng tiền của nguồn vốn - số tiền rút vốn theo kỳ, (ở kì 2 thì phải + kì 1 + kì 2 nếu có, ở kì 3 + kì 1 + 2 + 3 nếu có...)
                total -= money.Sum(s => s.MoneyNumber);
                total += moneyLoan.Sum(s => s.MoneyNumber);
            }
            decimal moneyPerDays = 0;
            switch (capitalRateType)
            {
                case CapitalRateTypeEnum.KPerMillion:
                    moneyPerDays = ((total / 1000000) * ((capitalDetail.InterestRate ?? 0) * 1000));
                    break;
                case CapitalRateTypeEnum.KperDay:
                    moneyPerDays = (capitalDetail.InterestRate ?? 0) * 1000;
                    break;
                case CapitalRateTypeEnum.KPerWeek:
                    moneyPerDays = ((capitalDetail.InterestRate ?? 0) * 1000) / 7;
                    break;
                case CapitalRateTypeEnum.PercentPerMonth:
                    moneyPerDays = (total * ((capitalDetail.InterestRate ?? 0) / 100)) / 30;
                    break;
                case CapitalRateTypeEnum.PercentPerMonthPeriodic:
                    moneyPerDays = (total * ((capitalDetail.InterestRate ?? 0) / 100));
                    break;
                case CapitalRateTypeEnum.PercentPerWeek:
                    moneyPerDays = (total * ((capitalDetail.InterestRate ?? 0) / 100)) / 7;
                    break;
                default:
                    break;
            }
            return moneyPerDays;
        }


        public MessageModels AddCapitalPayDay(CapitalPayDayModels objCapitalPayDays)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Trả tiền lãi thất bại!"
            };

            try
            {
                objCapitalPayDays.FromDate = objCapitalPayDays.FromDatePost;
                objCapitalPayDays.ToDate = objCapitalPayDays.ToDatePost;
                var capitalPayDays = _mapper.Map<CapitalPayDayModels, Tb_Capital_PayDay>(objCapitalPayDays);
                _unitOfWork.CapitalPayDayRepository.Insert(capitalPayDays);

                var result = _unitOfWork.SaveChanges();
                if (result > 0)
                {
                    var history = new HistoryModels
                    {
                        Content = message.Message.Replace(" thất bại!", ""),
                        DebitMoney = capitalPayDays.MoneyNumber + (capitalPayDays.MoneyOrther ?? 0),
                        ContractID = objCapitalPayDays.CapitalId,
                        TypeHistory = (int)DocumentTypeEnum.NguonVon,
                        StoreId = RDAuthorize.Store.Id
                    };
                    _history.AddHistory(history);

                    var cashBook = new CashBookModals
                    {
                        StoreId = RDAuthorize.Store.Id,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.NguonVon,
                        CreditAccount = history.HavingMoney ?? 0,
                        DebitAccount = history.DebitMoney ?? 0,
                        VoucherType = 2,
                        Customer = _unitOfWork.CapitalRepository.Find(s => s.Id == objCapitalPayDays.CapitalId)?.CustomerName ?? "",
                        Note = history.Content,
                        ContractId = history.ContractID
                    };

                    _cashBookServices.AddCashBook(cashBook);

                }
                message.Type = result > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = $"Trả tiền lãi thành công!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalServices --> AddCapitalPayDay ", ex);
            }

            return message;
        }

        public MessageModels DeleteCapitalPayDay(CapitalPayDayModels objCapitalPayDays)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = $"Hủy tiền lãi thất bại!"
            };

            try
            {
                var result = -1;
                var obj = _unitOfWork.CapitalPayDayRepository.Find(s => s.Id == objCapitalPayDays.Id);
                if (obj != null)
                {
                    _unitOfWork.CapitalPayDayRepository.Delete(obj);
                    result = _unitOfWork.SaveChanges();
                }

                if (result > 0)
                {
                    var history = new HistoryModels
                    {
                        Content = message.Message.Replace(" thất bại!", ""),
                        HavingMoney = objCapitalPayDays.MoneyNumber + (objCapitalPayDays.MoneyOrther ?? 0),
                        ContractID = objCapitalPayDays.CapitalId,
                        TypeHistory = (int)DocumentTypeEnum.NguonVon,
                        StoreId = RDAuthorize.Store.Id
                    };
                    _history.AddHistory(history);


                    var cashBook = new CashBookModals
                    {
                        StoreId = RDAuthorize.Store.Id,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.NguonVon,
                        CreditAccount = history.HavingMoney ?? 0,
                        DebitAccount = history.DebitMoney ?? 0,
                        VoucherType = 1,
                        Customer = _unitOfWork.CapitalRepository.Find(s => s.Id == objCapitalPayDays.CapitalId)?.CustomerName ?? "",
                        Note = history.Content,
                        ContractId = history.ContractID
                    };

                    _cashBookServices.AddCashBook(cashBook);
                }
                message.Type = result > 0 ? MessageTypeEnum.Success : MessageTypeEnum.Error;
                message.Message = $"Hủy tiền lãi thành công!";
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalServices --> DeleteCapitalPayDay ", ex);
            }

            return message;
        }
        public decimal GetTotalMoneyCapitalLoan(List<Tb_Capital_Loan> lstCapitalLoan, DateTime ngaydonglai)
        {
            int idx = 0;
            decimal total = 0;
            foreach (var item in lstCapitalLoan)
            {
                int totalDate = 0;
                totalDate = (idx == 0 ? (int)(item.LoadDate - ngaydonglai).TotalDays : (int)(item.LoadDate - lstCapitalLoan[idx].LoadDate).TotalDays) + 1;
                total += (item.MoneyNumber * totalDate);
                idx++;
            }
            return total;
        }
        public decimal GetMoneyPerDayPayDay(CapitalDetailModels capitalDetail, List<CapitalLoanModels> lstCapitalLoans, DateTime dtFromDate, DateTime dtToDate)
        {
            CapitalRateTypeEnum capitalRateType = (CapitalRateTypeEnum)capitalDetail.RateType;
            decimal moneyPerDay = 0;
            var totalDay = (int)((dtToDate - dtFromDate).TotalDays);
            if (totalDay < 0)
                totalDay = totalDay * -1;
            var totalMoneyCapital = capitalDetail.TotalMoney;
            if (capitalDetail.Method > (int)CapitalMethodEnum.VonDauTu)
            {
                switch (capitalRateType)
                {
                    case CapitalRateTypeEnum.KPerMillion:
                        for (int i = 0; i <= totalDay; i++)
                        {
                            var dt = dtFromDate.AddDays(i);
                            // Load từng ngày từ fromdate đến todate
                            // Kiểm tra xem ở ngày đó có rút vốn hay không, nếu có thì bắt đầu tính lại lãi theo totalmoney của nguồn vốn từ ngày tiếp theo
                            // ví dụ: tổng vốn là 100,000,000. Ngày rút vốn là 28/09: 10.000.000. Tính tiền lãi từ ngày 26/09 -> 30/09
                            // thì từ ngày 28/09 trở về ngày 26/09 sẽ tính lãi của số tiền 100tr. bắt đầu từ ngày 29 tính theo tổng tiền là 100tr - 10tr
                            if (lstCapitalLoans != null)
                                totalMoneyCapital = capitalDetail.TotalMoney - lstCapitalLoans.Where(s => s.LoadDate < dt && s.IsLoan == false).Sum(s => s.MoneyNumber);
                            moneyPerDay += ((totalMoneyCapital / 1000000) * ((capitalDetail.InterestRate ?? 0) * 1000));
                        }
                        break;
                    case CapitalRateTypeEnum.KperDay:
                        moneyPerDay = (capitalDetail.InterestRate ?? 0) * 1000 * (totalDay + 1);
                        break;
                    case CapitalRateTypeEnum.PercentPerMonth:
                    case CapitalRateTypeEnum.PercentPerMonthPeriodic:
                        for (int i = 0; i <= totalDay; i++)
                        {
                            var dt = dtFromDate.AddDays(i);
                            // Load từng ngày từ fromdate đến todate
                            // Kiểm tra xem ở ngày đó có rút vốn hay không, nếu có thì bắt đầu tính lại lãi theo totalmoney của nguồn vốn từ ngày tiếp theo
                            // ví dụ: tổng vốn là 100,000,000. Ngày rút vốn là 28/09: 10.000.000. Tính tiền lãi từ ngày 26/09 -> 30/09
                            // thì từ ngày 28/09 trở về ngày 26/09 sẽ tính lãi của số tiền 100tr. bắt đầu từ ngày 29 tính theo tổng tiền là 100tr - 10tr
                            if (lstCapitalLoans != null)
                                totalMoneyCapital = capitalDetail.TotalMoney - lstCapitalLoans.Where(s => s.LoadDate < dt && s.IsLoan == false).ToList().Sum(s => s.MoneyNumber);
                            moneyPerDay += (((totalMoneyCapital * (capitalDetail.InterestRate ?? 0)) / 100) / 30);
                        }
                        break;
                    case CapitalRateTypeEnum.PercentPerWeek:
                        var integer = (int)(totalDay / 7);
                        var surplus = totalDay % 7;
                        if (surplus > 0) integer += 1;
                        if (surplus < 0) integer -= 1;
                        if (lstCapitalLoans != null)
                            totalMoneyCapital = capitalDetail.TotalMoney - lstCapitalLoans.Where(s => s.IsLoan == false).ToList().Sum(s => s.MoneyNumber);
                        moneyPerDay = integer * ((totalMoneyCapital * (capitalDetail.InterestRate ?? 0)) / 100);
                        break;
                    case CapitalRateTypeEnum.KPerWeek:
                        integer = (int)(totalDay / 7);
                        surplus = totalDay % 7;
                        if (surplus != 0) integer += 1;
                        if (lstCapitalLoans != null)
                            totalMoneyCapital = capitalDetail.TotalMoney - lstCapitalLoans.Where(s => s.IsLoan == false).ToList().Sum(s => s.MoneyNumber);
                        moneyPerDay = integer * (capitalDetail.InterestRate ?? 0) * 1000;
                        break;
                    default:
                        break;
                }
            }
            return moneyPerDay;
        }

        public List<ExtentionContractModels> LoadListExtentionContract(List<ExtentionContractModels> lstData, DateTime toDate, int method)
        {
            CapitalMethodEnum capitalMethod = (CapitalMethodEnum)method;
            var listExtentionContract = new List<ExtentionContractModels>();
            var fromDate = toDate;
            foreach (var item in lstData)
            {
                var objExtention = new ExtentionContractModels
                {
                    ContractID = item.ContractID,
                    Id = item.Id,
                    Note = item.Note,
                    FromDate = fromDate,
                };
                switch (capitalMethod)
                {
                    case CapitalMethodEnum.VonDauTu:
                    case CapitalMethodEnum.LaiNgay:
                        objExtention.AddTime = item.AddTime;
                        break;
                    case CapitalMethodEnum.LaiThang:
                        objExtention.AddTime = item.AddTime * 30;
                        break;
                    case CapitalMethodEnum.LaiThangDinhKy:
                        var dtToDate = fromDate.AddMonths(item.AddTime);
                        var addTime = (int)(dtToDate - fromDate).TotalDays;
                        objExtention.AddTime = addTime;
                        break;
                    case CapitalMethodEnum.LaiTuanPT:
                    case CapitalMethodEnum.LaiTuanK:
                        objExtention.AddTime = item.AddTime * 7;
                        break;
                    default:
                        objExtention.AddTime = 0;
                        break;
                }
                objExtention.ToDate = fromDate.AddDays(objExtention.AddTime);
                listExtentionContract.Add(objExtention);
                fromDate = objExtention.ToDate;
            }
            return listExtentionContract;
        }

        public MessageModels CloseContract(int id, decimal moneyNumber)
        {
            var message = new MessageModels
            {
                Type = MessageTypeEnum.Error,
                Message = "Đóng hợp đồng thất bại"
            };

            try
            {
                var result = -1;
                var docType = (int)DocumentTypeEnum.NguonVon;
                var objCapital = _unitOfWork.CapitalRepository.Find(s => s.Id == id && s.StoreId == RDAuthorize.Store.Id && s.DocumentType == docType);
                if (objCapital != null)
                {
                    objCapital.IsClose = true;
                    objCapital.CloseDate = DateTime.Now;
                    objCapital.CloseUser = RDAuthorize.Username;
                    result = _unitOfWork.SaveChanges();
                }

                if (result > 0)
                {
                    var history = new HistoryModels
                    {
                        Content = message.Message.Replace(" thất bại!", ""),
                        DebitMoney = moneyNumber > 0 ? Math.Abs(moneyNumber) : 0,
                        HavingMoney = moneyNumber < 0 ? Math.Abs(moneyNumber) : 0,
                        ContractID = id,
                        TypeHistory = (int)DocumentTypeEnum.NguonVon,
                        StoreId = RDAuthorize.Store.Id
                    };
                    _history.AddHistory(history);

                    var cashBook = new CashBookModals
                    {
                        StoreId = RDAuthorize.Store.Id,
                        DocumentDate = DateTime.Now,
                        DocumentType = (int)DocumentTypeEnum.NguonVon,
                        CreditAccount = history.HavingMoney ?? 0,
                        DebitAccount = history.DebitMoney ?? 0,
                        VoucherType = moneyNumber < 0 ? 1 : 2,
                        Customer  = objCapital.CustomerName,
                        Note = history.Content,
                        ContractId = history.ContractID
                    };

                    _cashBookServices.AddCashBook(cashBook);

                    message.Type = MessageTypeEnum.Success;
                    message.Message = "Đóng hợp đồng thành công";
                }
            }
            catch (Exception ex)
            {
                PawnLog.Error("PawnCapitalServices --> CloseContract ", ex);
            }
            return message;
        }
    }
}
