using Pawn.Core.DataModel;
using Pawn.Core.IDataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawn.Core.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private bool _disposed = false;

        private PawnEntities _dbContext;
        public PawnEntities DbContext => _dbContext ?? (_dbContext = new PawnEntities());

        public UnitOfWork()
        {
            _dbContext = new PawnEntities();
        }

        private IRepository<Tb_Account> _accountRepository;
        private IRepository<Tb_AccountType> _accountTypeRepository;
        private IRepository<Tb_Menu> _menuRepository;
        private IRepository<Tb_Role> _roleRepository;
        private IRepository<Tb_Customer> _customerRepository;
        private IRepository<Tb_Store> _storeRepository;
        private IRepository<Tb_IncomeAndExpense> _incomeAndExpenseRepository;
        private IRepository<Tb_CashBook> _cashBookRepository;
        private IRepository<Tb_Capital> _capitalRepository;
        private IRepository<Tb_Capital_Loan> _capitalLoanRepository;
        private IRepository<Tb_Status> _statusRepository;
        private IRepository<Tb_History> _historyRepository;
        private IRepository<Tb_Document> _documentRepository;
        private IRepository<Tb_Capital_PayDay> _capitalPayDayRepository;
        private IRepository<Tb_MenuPermission> _menupermissionRepository;
        private IRepository<Tb_Account_Store> _storeAccountRepository;
        private IRepository<Tb_BHContract> _batHoRepository;
        private IRepository<Tb_BH_Pay> _batHoPayRepository;
        private IRepository<Tb_FileManagement> _fileManagementRepository;
        private IRepository<Tb_InfoInstallment> _infoInstallmentRepository;
        private IRepository<Tb_Debt> _debtRepository;
        private IRepository<Tb_PawnContract> _pawnContractRepository;
        private IRepository<Tb_PawnPay> _pawnPayRepository;
        private IRepository<Tb_ExtentionContract> _extentionContractRepository;
        private IRepository<Tb_Timer> _timerRepository;
        private IRepository<Tb_UserRole> _userRoleRepository;

        public IRepository<Tb_Account> AccountRepository => _accountRepository ?? (_accountRepository = new Repository<Tb_Account>(_dbContext));
        public IRepository<Tb_AccountType> AccountTypeRepository => _accountTypeRepository ?? (_accountTypeRepository = new Repository<Tb_AccountType>(_dbContext));
        public IRepository<Tb_Menu> MenuRepository => _menuRepository ?? (_menuRepository = new Repository<Tb_Menu>(_dbContext));
        public IRepository<Tb_Role> RoleRepository => _roleRepository ?? (_roleRepository = new Repository<Tb_Role>(_dbContext));
        public IRepository<Tb_Customer> CustomerRepository => _customerRepository ?? (_customerRepository = new Repository<Tb_Customer>(_dbContext));
        public IRepository<Tb_Store> StoreRepository => _storeRepository ?? (_storeRepository = new Repository<Tb_Store>(_dbContext));
        public IRepository<Tb_IncomeAndExpense> IncomeAndExpenseRepository => _incomeAndExpenseRepository ?? (_incomeAndExpenseRepository = new Repository<Tb_IncomeAndExpense>(_dbContext));
        public IRepository<Tb_CashBook> CashBookRepository => _cashBookRepository ?? (_cashBookRepository = new Repository<Tb_CashBook>(_dbContext));
        public IRepository<Tb_Capital> CapitalRepository => _capitalRepository ?? (_capitalRepository = new Repository<Tb_Capital>(_dbContext));
        public IRepository<Tb_Capital_Loan> CapitalLoanRepository => _capitalLoanRepository ?? (_capitalLoanRepository = new Repository<Tb_Capital_Loan>(_dbContext));
        public IRepository<Tb_Status> StatusRepository => _statusRepository ?? (_statusRepository = new Repository<Tb_Status>(_dbContext));
        public IRepository<Tb_History> HistoryRepository => _historyRepository ?? (_historyRepository = new Repository<Tb_History>(_dbContext));
        public IRepository<Tb_Document> DocumentRepository => _documentRepository ?? (_documentRepository = new Repository<Tb_Document>(_dbContext));
        public IRepository<Tb_Capital_PayDay> CapitalPayDayRepository => _capitalPayDayRepository ?? (_capitalPayDayRepository = new Repository<Tb_Capital_PayDay>(_dbContext));
        public IRepository<Tb_MenuPermission> MenuPermissionRepository => _menupermissionRepository ?? (_menupermissionRepository = new Repository<Tb_MenuPermission>(_dbContext));
        public IRepository<Tb_Account_Store> StoreAccountRepository => _storeAccountRepository ?? (_storeAccountRepository = new Repository<Tb_Account_Store>(_dbContext));
        public IRepository<Tb_BHContract> BatHoRepository => _batHoRepository ?? (_batHoRepository = new Repository<Tb_BHContract>(_dbContext));
        public IRepository<Tb_BH_Pay> BatHoPayRepository => _batHoPayRepository ?? (_batHoPayRepository = new Repository<Tb_BH_Pay>(_dbContext));
        public IRepository<Tb_FileManagement> FileManagementRepository => _fileManagementRepository ?? (_fileManagementRepository = new Repository<Tb_FileManagement>(_dbContext));
        public IRepository<Tb_InfoInstallment> InfoInstallmentRepository => _infoInstallmentRepository ?? (_infoInstallmentRepository = new Repository<Tb_InfoInstallment>(_dbContext));
        public IRepository<Tb_Debt> DebtRepository => _debtRepository ?? (_debtRepository = new Repository<Tb_Debt>(_dbContext));
        public IRepository<Tb_PawnContract> PawnContractRepository => _pawnContractRepository ?? (_pawnContractRepository = new Repository<Tb_PawnContract>(_dbContext));
        public IRepository<Tb_PawnPay> PawnPayRepository => _pawnPayRepository ?? (_pawnPayRepository = new Repository<Tb_PawnPay>(_dbContext));
        public IRepository<Tb_ExtentionContract> ExtentionContractRepository => _extentionContractRepository ?? (_extentionContractRepository = new Repository<Tb_ExtentionContract>(_dbContext));
        public IRepository<Tb_Timer> TimerRepository => _timerRepository ?? (_timerRepository = new Repository<Tb_Timer>(_dbContext));

        public IRepository<Tb_UserRole> UserRoleRepository => _userRoleRepository ?? (_userRoleRepository = new Repository<Tb_UserRole>(_dbContext));
        
        public int SaveChanges()
        {
            CheckIsDisposed();
            return _dbContext.SaveChanges();
        }

        public IEnumerable<T> ExecStoreProdure<T>(string strExcecStore, params object[] param) => _dbContext.Database.SqlQuery<T>(strExcecStore, param);
        
        public int ExcecStoreProdure(string strExcecStore, params object[] param) => _dbContext.Database.ExecuteSqlCommand(strExcecStore, param);
        public string GetConnectionString() => _dbContext.Database.Connection.ConnectionString;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _dbContext?.Dispose();
            }
            _disposed = true;
        }

        private void CheckIsDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

       
    }
}
