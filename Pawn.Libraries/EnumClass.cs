using System.ComponentModel;
using static Pawn.Libraries.Utility;

namespace Pawn.Libraries
{

    public enum AccountType
    {
        Root = -1,
        Admin = 1,
        User = 2
    }

    public enum Result
    {
        Success = 1,
        Error = -1,
        Duplicate = 0
    }

    public enum DocumentTypeEnum
    {
        NguonVon = 1,
        ThuChiHoatDong = 2,
        BatHo = 3,
        RemindB = 6,
        RemindV = 7,
        VayLai = 4,
        CamDo = 5
    }

    public enum VocherTypeEnum
    {
        PhieuThu = 1,
        PhieuChi = 2,
        None = 0
    }

    public enum MessageTypeEnum
    {
        Information = 1,
        Success = 2,
        Warning = 3,
        Error = 4,
    }

    public enum CapitalRateTypeEnum
    {
        KPerMillion = 0,
        KperDay = 1,
        PercentPerMonth = 2,
        PercentPerMonthPeriodic = 3,
        PercentPerWeek = 4,
        KPerWeek = 5,
    }

    public enum CapitalMethodEnum
    {
        VonDauTu = 14,
        LaiNgay = 15,
        LaiThang = 16,
        LaiThangDinhKy = 17,
        LaiTuanPT = 18,
        LaiTuanK = 19,

    }

    public enum MethodTypeEnum
    {
        MoneyPerDay = 1,
        PercentPerMonthDay = 2,
        PercentPerMonth30 = 3,
        PercentPerWeek = 4,
        MoneyPerWeek = 5,
    }

    public enum PawnStatusEnum
    {
        DangVay = 1,
        NoLai = 2,
        QuaHan = 3,
        TraGoc = 4,
        NoXau = 5
    }

    public enum HistoryType
    {
        Capital = 0,
        BatHo = 1,
    }

    public enum TimePawnDate
    {
        _40Ngay = 40,
        _50Ngay = 50,
        _60Ngay = 60,
        _100Ngay = 100
    }

    public enum StatusTypeCapital
    {
        DenNgay = 1,
        ChamNgay = 2,
        NoLai = 3
    }

    public enum StatusContractEnum
    {
        [Description("Tất cả hợp đồng")]
        TatCaHD = -1,
        [Description("Tất cả hợp đồng đang vay")]
        TatCaHDDangvay = 1,
        [Description("Hợp đồng đúng hẹn")]
        DungHen = 2,
        [Description("Hợp đồng chậm họ")]
        ChamHo = 3,
        [Description("Hợp đồng quá hạn")]
        QuaHan = 4,
        [Description("Hợp đồng nợ xấu")]
        NoXau = 5,
        [Description("Hợp đồng kết thúc")]
        KetThuc = 6,
        [Description("Hợp đồng đã xóa")]
        DaXoa = 7,
        [Description("Hợp đồng ngày mai đóng họ")]
        NgayMaiDongHo = 8
    }

    public enum StatusContractPawnEnum
    {
        [Description("Tất cả các hợp đồng")]
        AllHd = 0,
        [Description("Hợp đồng đang vay")]
        HDDangvay = 1,
        [Description("Hợp đồng đúng hẹn")]
        DungHen = 2,
        [Description("Hợp đồng quá hạn")]
        QuaHan = 3,
        [Description("Hợp đồng chậm lãi")]
        ChamLai = 4,
        [Description("Hợp đồng nợ xấu")]
        NoXau = 5,
        [Description("Hợp đồng kết thúc")]
        KetThuc = 6,
        [Description("Hợp đồng đã xóa")]
        DaXoa = 7
    }

    public enum InterestRateTypeEnum
    {
        [Description("Lãi ngày")]
        LaiNgay = 1,
        [Description("Lãi tháng (%) (Định kỳ)")]
        LaiThangDinhKy = 2,
        [Description("Lãi tháng (%) (30 ngày)")]
        LaiThang30Ngay = 3,
        [Description("Lãi tuần (%)")]
        LaiTuanPhanTram = 4,
        [Description("Lãi tuần (k)")]
        LaiTuanTheoK = 5

    }

    public class RoleEnum
    {
        public const int AllRoles = -1;
        [Description("Root")]
        public const int Root = 2;
        [Description("Admin")]
        public const int Admin = 3;
        [Description("Store")]
        public const int Store = 4;
        [Description("Staff")]
        public const int UserVN = 5;
        [Description("Staff2")]
        public const int UserUSA = 6;

    }

    public enum InterestPaid
    {
        ToDay = 0,
        Tomorrow = 1
    }

    public enum NotesEnum
    {
        [Description("Giải ngân hợp đồng Bát họ")]
        [DocumentType(3)]
        GiaiNganHopDongBatHo = 2,

        [Description("Tiền giới thiệu HD Bát họ")]
        [DocumentType(3)]
        TienGioiThieuHopDongBatHo = 3,

        [Description("Tiền dịch vụ HD Bát họ")]
        [DocumentType(3)]
        TienDichVuHopDongBatHo = 4,

        [Description("Đóng tiền họ")]
        [DocumentType(3)]
        DongTienHo = 5,

        [Description("Hủy đóng tiền họ")]
        [DocumentType(3)]
        HuyDongTienHo = 6,

        [Description("Giải ngân hợp đồng Vay lãi")]
        [DocumentType(4)]
        GiaiNganHopDongVayLai = 7,

        [Description("Tiền giới thiệu HD")]
        [DocumentType(4)]
        TienGioiThieuHopDongVaiLai = 8,

        [Description("Tiền dịch vụ HD Vay lãi")]
        [DocumentType(4)]
        TienDichVuHopDongVayLai = 9,

        [Description("Đóng lãi")]
        [DocumentType(4)]
        DongLai = 10,

        [Description("Hủy đóng lãi")]
        [DocumentType(4)]
        HuyDongLai = 11,

        [Description("Đóng hợp đồng")]
        [DocumentType(4)]
        DongHopDongVaiLai = 13,

        [Description("Xóa hợp đồng Vay lãi")]
        [DocumentType(4)]
        XoaHDVayLai = 12,

        [Description("Xóa hợp đồng Bát Họ")]
        [DocumentType(4)]
        XoaHDBatHo = 14

    }
}
