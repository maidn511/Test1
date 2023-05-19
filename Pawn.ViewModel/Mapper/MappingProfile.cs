using AutoMapper;
using Pawn.Core.DataModel;
using Pawn.ViewModel.Models;

namespace Pawn.ViewModel.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Map Entity to Model
            CreateMap<Tb_Account, AccountModels>().ReverseMap();
            CreateMap<MenuModels, Tb_Menu>().ReverseMap(); ;
            CreateMap<RoleModels, Tb_Role>().ReverseMap();
            CreateMap<CustomerModels, Tb_Customer>().ReverseMap();
            CreateMap<PawnStoreModels, Tb_Store>().ReverseMap();
            CreateMap<IncomeAndExpenseModels, Tb_IncomeAndExpense>().ReverseMap();
            CreateMap<CapitalModels, Tb_Capital>().ReverseMap();
            CreateMap<CapitalLoanModels, Tb_Capital_Loan>().ReverseMap();
            CreateMap<HistoryModels, Tb_History>().ReverseMap();
            CreateMap<CashBookModals, Tb_CashBook>().ReverseMap();
            CreateMap<CapitalPayDayModels, Tb_Capital_PayDay>().ReverseMap();
            CreateMap<DocumentModals, Tb_Document>().ReverseMap();
            CreateMap<BatHoModels, Tb_BHContract>().ReverseMap();
            CreateMap<BatHoPayModels, Tb_BH_Pay>().ReverseMap();
            CreateMap<FileManagementModels, Tb_FileManagement>().ReverseMap();
            CreateMap<DebtModels, Tb_Debt>().ReverseMap();
            CreateMap<PawnContractModels, Tb_PawnContract>().ReverseMap();
            CreateMap<PawnPayModels, Tb_PawnPay>().ReverseMap();
            CreateMap<ExtentionContractModels, Tb_ExtentionContract>().ReverseMap();
            CreateMap<TimerModels, Tb_Timer>().ReverseMap();
        }
    }
}
