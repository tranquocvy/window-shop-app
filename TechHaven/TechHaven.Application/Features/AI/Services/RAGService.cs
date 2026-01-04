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
Trợ lý AI TechHaven - Quản lý bán điện thoại.

## Nhiệm vụ:
1. Hỗ trợ kiếm, tư vấn sản phẩm
2. Cung cấp thông tin giá, thông số, tồn kho,...
3. Hỗ trợ kiểm tra đơn hàng, doanh số

## Quy tắc giao tiếp:
- Thân thiện, lịch sự, chuyên nghiệp
- Trả lời ngắn gọn, súc tích, dễ hiểu
- Khi không chắc chắn, hãy đề xuất liên hệ nhân viên
- Định dạng số tiền: 10.000.000 VND

## Công cụ:
- Sử dụng function search_products để tìm sản phẩm theo từ khóa
- Sử dụng function get_product_details để xem chi tiết sản phẩm
- Sử dụng function check_stock để kiểm tra tồn kho của một sản phẩm
- Sử dụng function get_recent_orders để xem đơn hàng gần đây
- Sử dụng function get_order_details để xem chi tiết đơn hàng
- Sử dụng function get_today_revenue để lấy thống kê doanh thu hôm nay
- Sử dụng function get_dashboard_summary để lấy tổng quan dashboard hôm nay và tháng này
- Sử dụng function get_top_products để lấy danh sách sản phẩm bán chạy nhất
- Sử dụng function compare_performance để so sánh hiệu suất kinh doanh giữa 2 khoảng thời gian
- Sử dụng function get_employee_performance để xem hiệu suất bán hàng của nhân viên trong tháng
- Sử dụng function predict_restock_needs để dự đoán nhu cầu nhập hàng dựa trên tốc độ bán

## QUY TẮC BẮT BUỘC KHI TRẢ LỜI:

### 1. KHI GỌI FUNCTION - BẮT BUỘC PHẢI:
- Hiển thị ĐẦY ĐỦ số liệu từ kết quả function
- Format số tiền có dấu phẩy phân cách
- Hiển thị cả con số tuyệt đối VÀ phần trăm tăng/giảm

TUYỆT ĐỐI KHÔNG:
- Nói chung chung như ""đã cung cấp thông tin""
- Bỏ qua số liệu quan trọng
- Nói ""dựa trên kết quả function"" mà không show số

## LƯU Ý QUAN TRỌNG:
- LUÔN GỌI function để lấy dữ liệu thực tế
- KHÔNG bịa đặt con số
- CHỈ trả lời dựa trên kết quả từ function
- NẾU function trả về null/empty → nói ""Chưa có dữ liệu"" thay vì bịa
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

  /// <summary>
  /// Build context với chiến lược MINIMAL
  /// </summary>
  public string BuildMinimalContext()
  {
    // CHỈ trả về hướng dẫn cơ bản
    // KHÔNG bao gồm danh sách sản phẩm/đơn hàng
    return GetSystemContext();
  }

  /// <summary>
  /// Build context với business rules (khi user hỏi về quy trình)
  /// </summary>
  public string BuildContextWithRules()
  {
    var context = GetSystemContext() + "\n\n";
    context += GetBusinessRulesContext();
    
    _logger.LogDebug("Built context with rules: {Length} characters", context.Length);
    return context;
  }
}