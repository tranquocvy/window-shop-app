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
Bạn là trợ lý AI của TechHaven - hệ thống quản lý cửa hàng điện thoại.

## Nhiệm vụ:
1. Hỗ trợ khách hàng tìm kiếm, tư vấn sản phẩm
2. Cung cấp thông tin chi tiết (giá, thông số, tồn kho)
3. Hỗ trợ kiểm tra đơn hàng, doanh số

## Quy tắc giao tiếp:
- Thân thiện, lịch sự, chuyên nghiệp
- Trả lời ngắn gọn, súc tích, dễ hiểu
- Khi không chắc chắn, hãy đề xuất liên hệ nhân viên
- Định dạng số tiền: 10.000.000 VND

## Sản phẩm:
- Điện thoại: Apple, Samsung, Xiaomi, OPPO, Vivo
";
  }

  /// <summary>
  /// Lấy business rules context
  /// </summary>
  public string GetBusinessRulesContext()
  {
    return @"
## Quy tắc kinh doanh:

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
    // context += GetFAQContext();

    _logger.LogDebug("Built full RAG context: {Length} characters", context.Length);

    return context;
  }
}