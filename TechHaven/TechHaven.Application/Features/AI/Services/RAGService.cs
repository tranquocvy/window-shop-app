using Microsoft.Extensions.Logging;

public class RAGService
{
  private readonly ILogger<RAGService> _logger;

  public RAGService(ILogger<RAGService> logger)
  {
    _logger = logger;
  }

  /// <summary>
  /// Lấy system context - thông tin cơ bản về hệ thống
  /// </summary>
  public string GetSystemContext()
  {
    return $@"
Bạn là trợ lý AI của TechHaven - một hệ thống quản lý cửa hàng điện thoại.

## Thông tin cửa hàng:
- Tên: TechHaven
- Thời gian hiện tại: ${DateTime.Now}

## Nhiệm vụ của bạn:
1. Hỗ trợ khách hàng tìm kiếm sản phẩm
2. Cung cấp thông tin chi tiết về sản phẩm (giá, thông số, tồn kho)
3. Tư vấn sản phẩm phù hợp với nhu cầu
4. Hỗ trợ nhân viên kiểm tra đơn hàng, doanh số

## Quy tắc giao tiếp:
- Luôn thân thiện, lịch sự và chuyên nghiệp
- Trả lời ngắn gọn, súc tích
- Khi không chắc chắn, hãy thừa nhận và đề xuất liên hệ nhân viên
- Sử dụng tiếng Việt tự nhiên, không dùng từ ngữ khó hiểu
- Định dạng số tiền theo chuẩn VN: 10.000.000 VND

## Các sản phẩm chính:
- Điện thoại các hãng: Apple (iPhone), Samsung, Xiaomi, OPPO, Vivo
- Phụ kiện: Tai nghe, sạc, ốp lưng, miếng dán
- Dịch vụ: Bảo hành, sửa chữa, trade-in

## Chính sách:
- Bảo hành 12 tháng với sản phẩm mới
- Đổi trả trong 7 ngày (nếu còn nguyên seal)
- Miễn phí vận chuyển đơn từ 500.000 VND
- Giảm 10% cho sinh viên (xuất trình thẻ)
";
  }

  /// <summary>
  /// Lấy business rules context
  /// </summary>
  public string GetBusinessRulesContext()
  {
    return @"
## Quy tắc kinh doanh:

### Loại khách hàng:
- Regular (Thường): Không giảm giá
- Student (Sinh viên): Giảm 10% (cần thẻ SV)
- VIP: Giảm 5% + ưu tiên hỗ trợ

### Trạng thái đơn hàng:
- Pending: Chờ xử lý
- Processing: Đang xử lý
- Completed: Hoàn thành
- Cancelled: Đã hủy
- Returned: Đã trả hàng

### Hoa hồng nhân viên:
- Doanh số < 50 triệu: 2%
- 50-100 triệu: 3%
- > 100 triệu: 5%

### Cảnh báo tồn kho:
- Sản phẩm có số lượng <= 5: Cảnh báo sắp hết
- Số lượng = 0: Hết hàng
";
  }

  /// <summary>
  /// Lấy FAQ context
  /// </summary>
  public string GetFAQContext()
  {
    return @"
## Câu hỏi thường gặp:

**Q: Giá có bao gồm VAT chưa?**
A: Tất cả giá đã bao gồm VAT 10%.

**Q: Cửa hàng có giao hàng không?**
A: Có, miễn phí với đơn từ 500k trong nội thành TP.HCM.

**Q: Chính sách đổi trả như thế nào?**
A: Đổi trả trong 7 ngày nếu sản phẩm còn nguyên seal, đầy đủ phụ kiện.

**Q: Có trade-in máy cũ không?**
A: Có, đánh giá trực tiếp tại cửa hàng.

**Q: Thanh toán như thế nào?**
A: Tiền mặt, chuyển khoản, thẻ tín dụng/ghi nợ.

**Q: Có bán trả góp không?**
A: Có, hỗ trợ qua các công ty tài chính (0% lãi suất).
";
  }

  /// <summary>
  /// Build full context cho AI
  /// </summary>
  public string BuildFullContext()
  {
    var context = GetSystemContext() + "\n\n";
    context += GetBusinessRulesContext() + "\n\n";
    context += GetFAQContext();

    _logger.LogDebug("Built full RAG context: {Length} characters", context.Length);

    return context;
  }
}