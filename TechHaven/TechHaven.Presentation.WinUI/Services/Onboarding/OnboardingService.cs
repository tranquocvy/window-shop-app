using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using TechHaven.Presentation.WinUI.Views;

namespace TechHaven.Presentation.WinUI.Services.Onboarding
{
    public interface IOnboardingService
    {
        Task RunAsync(ShellWindow shell);
    }

    public class OnboardingService : IOnboardingService
    {
        private class OnboardingStep
        {
            public Type? PageType { get; set; }
            public string? TargetElementName { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Subtitle { get; set; } = string.Empty;
        }

        private bool _isSkipped = false;
        private int _currentStepIndex = 0;
        private List<OnboardingStep> _steps = new List<OnboardingStep>();

        public async Task RunAsync(ShellWindow shell)
        {
            // Define all onboarding steps
            _steps = new List<OnboardingStep>
            {
                new OnboardingStep
                {
                    PageType = typeof(DashboardPage),
                    Title = "Bảng điều khiển",
                    Subtitle = "Tổng quan doanh thu, đơn hàng và thông tin nhanh."
                },
                new OnboardingStep
                {
                    PageType = typeof(ProductPage),
                    Title = "Sản phẩm",
                    Subtitle = "Quản lý danh sách, tồn kho và chi tiết sản phẩm."
                },
                new OnboardingStep
                {
                    PageType = typeof(ProductPage),
                    TargetElementName = "SearchBox",
                    Title = "Tìm kiếm",
                    Subtitle = "Nhập tên để tìm nhanh sản phẩm."
                },
                new OnboardingStep
                {
                    PageType = typeof(ProductPage),
                    TargetElementName = "AddButton",
                    Title = "Thêm sản phẩm",
                    Subtitle = "Tạo sản phẩm mới từ đây."
                },
                new OnboardingStep
                {
                    PageType = typeof(OrderPage),
                    Title = "Đơn hàng",
                    Subtitle = "Xem, lọc và xử lý đơn hàng."
                },
                new OnboardingStep
                {
                    PageType = typeof(ReportPage),
                    Title = "Báo cáo",
                    Subtitle = "Xem biểu đồ và xuất báo cáo."
                },
                new OnboardingStep
                {
                    PageType = typeof(ReportPage),
                    TargetElementName = "ExportButton|TabletExportButton",
                    Title = "Xuất báo cáo",
                    Subtitle = "Nhấn để xuất báo cáo theo bộ lọc hiện tại."
                },
                new OnboardingStep
                {
                    PageType = typeof(ReportPage),
                    TargetElementName = "PeriodCombo|TabletPeriodCombo",
                    Title = "Theo kỳ",
                    Subtitle = "Chọn Ngày/Tháng/Năm để đổi kỳ báo cáo."
                },
                new OnboardingStep
                {
                    PageType = typeof(ReportPage),
                    TargetElementName = "TabletSearchButton",
                    Title = "Xem báo cáo",
                    Subtitle = "Nhấn để xem dữ liệu theo bộ lọc đã chọn."
                },
                new OnboardingStep
                {
                    PageType = typeof(SettingPage),
                    Title = "Cài đặt",
                    Subtitle = "Cấu hình server, chủ đề và tuỳ chọn."
                }
            };

            _isSkipped = false;
            _currentStepIndex = 0;

            // Navigate through steps
            while (_currentStepIndex < _steps.Count && !_isSkipped)
            {
                var step = _steps[_currentStepIndex];
                var result = await ShowStepAsync(shell, step);

                if (result == StepResult.Skip)
                {
                    _isSkipped = true;
                    break;
                }
                else if (result == StepResult.Previous)
                {
                    if (_currentStepIndex > 0)
                    {
                        _currentStepIndex--;
                    }
                }
                else if (result == StepResult.Next)
                {
                    _currentStepIndex++;
                }
            }
        }

        private enum StepResult
        {
            Next,
            Previous,
            Skip
        }

        private async Task<StepResult> ShowStepAsync(ShellWindow shell, OnboardingStep step)
        {
            try
            {
                // Navigate to page if specified
                if (step.PageType != null)
                {
                    shell.NavigateTo(step.PageType);
                    // Give time for page to load
                    await Task.Delay(300);
                }
            }
            catch (Exception ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"OnboardingService: Navigation error: {ex}");
            }

            var root = shell.GetCurrentPageRoot();
            if (root == null) 
            {
                // Fallback to dialog if can't get root
                System.Diagnostics.Debug.WriteLine("OnboardingService: No page root, using dialog fallback");
                var result = await ShowDialogAsync(shell, step);
                return result;
            }

            // Find target element if specified
            FrameworkElement? target = null;
            if (!string.IsNullOrEmpty(step.TargetElementName))
            {
                try
                {
                    // Support multiple possible names separated by |
                    var possibleNames = step.TargetElementName.Split('|');
                    foreach (var name in possibleNames)
                    {
                        target = (root as FrameworkElement)?.FindName(name.Trim()) as FrameworkElement;
                        if (target != null) break;
                    }
                    
                    if (target == null)
                    {
                        System.Diagnostics.Debug.WriteLine($"OnboardingService: Target element '{step.TargetElementName}' not found");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnboardingService: Error finding target element: {ex}");
                    target = null;
                }
            }

            // Try TeachingTip first for better UX if we have a target
            if (target != null && _currentStepIndex == 0)
            {
                try
                {
                    // For first step, use TeachingTip without Previous button
                    var tipResult = await ShowTeachingTipAsync(shell, root, target, step, false);
                    return tipResult;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"OnboardingService: TeachingTip error: {ex}, fallback to dialog");
                }
            }

            // Use ContentDialog for better control over buttons including Previous
            var stepResult = await ShowDialogAsync(shell, step);
            return stepResult;
        }

        private async Task<StepResult> ShowTeachingTipAsync(ShellWindow shell, FrameworkElement root, FrameworkElement? target, OnboardingStep step, bool showPrevious)
        {
            var progressText = $"Bước {_currentStepIndex + 1}/{_steps.Count}";
            
            var tip = new TeachingTip
            {
                Title = step.Title,
                Subtitle = $"{step.Subtitle}\n\n{progressText}",
                PreferredPlacement = TeachingTipPlacementMode.Bottom,
                IsOpen = false,
                ActionButtonContent = _currentStepIndex < _steps.Count - 1 ? "Tiếp" : "Hoàn thành",
                CloseButtonContent = "Bỏ qua"
            };

            if (target != null)
            {
                tip.Target = target;
            }

            // Create custom content with Previous button if needed
            if (showPrevious && _currentStepIndex > 0)
            {
                var stack = new StackPanel { Spacing = 12 };
                
                var subtitleText = new TextBlock 
                { 
                    Text = $"{step.Subtitle}\n\n{progressText}",
                    TextWrapping = TextWrapping.Wrap 
                };
                
                var buttonPanel = new StackPanel 
                { 
                    Orientation = Orientation.Horizontal, 
                    Spacing = 8,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 12, 0, 0)
                };

                var previousBtn = new Button 
                { 
                    Content = "← Quay lại",
                    Style = Application.Current.Resources["AccentButtonStyle"] as Style
                };

                buttonPanel.Children.Add(previousBtn);
                stack.Children.Add(subtitleText);
                stack.Children.Add(buttonPanel);

                // Note: This approach won't work well with TeachingTip
                // We'll use ContentDialog instead when Previous is needed
            }

            // Add tip to visual tree
            try
            {
                if (root is Panel p)
                {
                    p.Children.Add(tip);
                }
                else if (root is Grid g)
                {
                    g.Children.Add(tip);
                }
                else
                {
                    var dialogResult = await ShowDialogAsync(shell, step);
                    return dialogResult;
                }
            }
            catch
            {
                var dialogResult = await ShowDialogAsync(shell, step);
                return dialogResult;
            }

            tip.IsOpen = true;

            var tcs = new TaskCompletionSource<StepResult>();
            
            // Handle Next/Finish button
            tip.ActionButtonClick += (s, e) => 
            { 
                tip.IsOpen = false; 
                tcs.TrySetResult(StepResult.Next); 
            };

            // Handle Skip button
            tip.CloseButtonClick += (s, e) => 
            { 
                tip.IsOpen = false; 
                tcs.TrySetResult(StepResult.Skip); 
            };

            // Handle manual close (treat as skip)
            tip.Closed += (s, e) => 
            { 
                tcs.TrySetResult(StepResult.Skip); 
            };

            var result = await tcs.Task;

            // Remove tip from visual tree
            try
            {
                if (root is Panel p2)
                    p2.Children.Remove(tip);
                else if (root is Grid g2)
                    g2.Children.Remove(tip);
            }
            catch { }

            return result;
        }

        private async Task<StepResult> ShowDialogAsync(ShellWindow shell, OnboardingStep step)
        {
            var progressText = $"Bước {_currentStepIndex + 1}/{_steps.Count}";
            var contentText = $"{step.Subtitle}\n\n{progressText}";

            var dialog = new ContentDialog
            {
                Title = step.Title,
                Content = contentText,
                PrimaryButtonText = _currentStepIndex < _steps.Count - 1 ? "Tiếp" : "Hoàn thành",
                CloseButtonText = "Bỏ qua",
                XamlRoot = (shell.Content as FrameworkElement)?.XamlRoot
            };

            // Add Previous button if not first step
            if (_currentStepIndex > 0)
            {
                dialog.SecondaryButtonText = "Quay lại";
            }

            var dialogResult = await dialog.ShowAsync();

            if (dialogResult == ContentDialogResult.Primary)
                return StepResult.Next;
            else if (dialogResult == ContentDialogResult.Secondary)
                return StepResult.Previous;
            else
                return StepResult.Skip;
        }
    }
}
