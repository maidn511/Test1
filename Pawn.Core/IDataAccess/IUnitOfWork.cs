using Pawn.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawn.Core.IDataAccess
{
    public interface IUnitOfWork : IDisposable
    {
        IEnumerable<T> ExecStoreProdure<T>(string strExcecStore, params object[] param);
        int ExcecStoreProdure(string strExcecStore, params object[] param);
        PawnEntities DbContext { get; }

        int SaveChanges();
        string GetConnectionString();
        IRepository<Tb_Account> AccountRepository { get; }
        IRepository<Tb_AccountType> AccountTypeRepository { get; }
        IRepository<Tb_Menu> MenuRepository { get; }
        IRepository<Tb_Role> RoleRepository { get; }
        IRepository<Tb_Customer> CustomerRepository { get; }
        IRepository<Tb_Store> StoreRepository { get; }
        //IRepository<Tb_BallotType> BallotTypeRepository { get; }
        IRepository<Tb_IncomeAndExpense> IncomeAndExpenseRepository { get; }
        IRepository<Tb_Capital> CapitalRepository { get; }
        IRepository<Tb_Capital_Loan> CapitalLoanRepository { get; }
        IRepository<Tb_CashBook> CashBookRepository { get; }
        IRepository<Tb_Status> StatusRepository { get; }
        IRepository<Tb_History> HistoryRepository { get; }
        IRepository<Tb_Document> DocumentRepository { get; }
        IRepository<Tb_Capital_PayDay> CapitalPayDayRepository { get; }
        IRepository<Tb_MenuPermission> MenuPermissionRepository { get; }
        IRepository<Tb_Account_Store> StoreAccountRepository { get; }
        IRepository<Tb_BHContract> BatHoRepository { get; }
        IRepository<Tb_BH_Pay> BatHoPayRepository { get; }
        IRepository<Tb_FileManagement> FileManagementRepository { get; }
        IRepository<Tb_InfoInstallment> InfoInstallmentRepository { get; }
        IRepository<Tb_Debt> DebtRepository { get; }
        IRepository<Tb_PawnContract> PawnContractRepository { get; }
        IRepository<Tb_PawnPay> PawnPayRepository { get; }
        IRepository<Tb_ExtentionContract> ExtentionContractRepository { get; }
        IRepository<Tb_Timer> TimerRepository { get; }

        IRepository<Tb_UserRole> UserRoleRepository { get; }
    }
}
