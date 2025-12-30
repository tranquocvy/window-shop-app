using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TechHaven.Presentation.WinUI.Helpers
{
    /// <summary>
    /// Helper class to parse and render Markdown content into WinUI controls
    /// </summary>
    public static class MarkdownHelper
    {
        /// <summary>
        /// Parse markdown text and create a formatted StackPanel with WinUI controls
        /// </summary>
        public static StackPanel ParseMarkdown(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return new StackPanel();

            var container = new StackPanel { Spacing = 8 };
            var lines = markdown.Split('\n');
            
            int i = 0;
            while (i < lines.Length)
            {
                var line = lines[i].TrimEnd('\r');
                
                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                // Code block (```)
                if (line.TrimStart().StartsWith("```"))
                {
                    var codeBlock = ParseCodeBlock(lines, ref i);
                    if (codeBlock != null)
                        container.Children.Add(codeBlock);
                    continue;
                }

                // Headers
                if (line.StartsWith("#"))
                {
                    container.Children.Add(ParseHeader(line));
                    i++;
                    continue;
                }

                // Bullet list
                if (line.TrimStart().StartsWith("- ") || line.TrimStart().StartsWith("* "))
                {
                    var list = ParseList(lines, ref i);
                    container.Children.Add(list);
                    continue;
                }

                // Numbered list
                if (Regex.IsMatch(line.TrimStart(), @"^\d+\.\s"))
                {
                    var list = ParseNumberedList(lines, ref i);
                    container.Children.Add(list);
                    continue;
                }

                // Table (check for | characters)
                if (line.TrimStart().StartsWith("|") || line.Contains("|"))
                {
                    var table = ParseTable(lines, ref i);
                    if (table != null)
                    {
                        container.Children.Add(table);
                        continue;
                    }
                }

                // Horizontal rule
                if (line.Trim() == "---" || line.Trim() == "***")
                {
                    container.Children.Add(new Border
                    {
                        Height = 1,
                        Background = new SolidColorBrush(Colors.Gray),
                        Opacity = 0.3,
                        Margin = new Thickness(0, 8, 0, 8)
                    });
                    i++;
                    continue;
                }

                // Blockquote (>)
                if (line.TrimStart().StartsWith(">"))
                {
                    var blockquote = ParseBlockquote(lines, ref i);
                    container.Children.Add(blockquote);
                    continue;
                }

                // Regular paragraph
                container.Children.Add(ParseParagraph(line));
                i++;
            }

            return container;
        }

        /// <summary>
        /// Parse markdown and return a RichTextBlock (more advanced formatting)
        /// </summary>
        public static RichTextBlock ParseMarkdownToRichText(string markdown)
        {
            var richTextBlock = new RichTextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true
            };

            if (string.IsNullOrWhiteSpace(markdown))
                return richTextBlock;

            var lines = markdown.Split('\n');
            int i = 0;

            while (i < lines.Length)
            {
                var line = lines[i].TrimEnd('\r');

                if (string.IsNullOrWhiteSpace(line))
                {
                    i++;
                    continue;
                }

                // Headers
                if (line.StartsWith("#"))
                {
                    var paragraph = CreateHeaderParagraph(line);
                    richTextBlock.Blocks.Add(paragraph);
                    i++;
                    continue;
                }

                // Regular paragraph with inline formatting
                var para = CreateParagraphWithInlineFormatting(line);
                richTextBlock.Blocks.Add(para);
                i++;
            }

            return richTextBlock;
        }

        private static Border ParseCodeBlock(string[] lines, ref int index)
        {
            var startLine = lines[index].TrimStart();
            var language = startLine.Length > 3 ? startLine.Substring(3).Trim() : "";
            index++; // Skip opening ```

            var codeLines = new List<string>();
            while (index < lines.Length)
            {
                var line = lines[index].TrimEnd('\r');
                if (line.TrimStart().StartsWith("```"))
                {
                    index++; // Skip closing ```
                    break;
                }
                codeLines.Add(line);
                index++;
            }

            var codeText = string.Join("\n", codeLines);

            var textBlock = new TextBlock
            {
                Text = codeText,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 13,
                Padding = new Thickness(12),
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true
            };

            var border = new Border
            {
                Background = GetCodeBlockBrush(),
                BorderBrush = GetCodeBlockBorderBrush(),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Margin = new Thickness(0, 4, 0, 4),
                Child = textBlock
            };

            return border;
        }

        private static TextBlock ParseHeader(string line)
        {
            int level = 0;
            while (level < line.Length && line[level] == '#')
                level++;

            var text = line.Substring(level).Trim();

            var textBlock = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, level == 1 ? 16 : 12, 0, 8)
            };

            // Set font size based on header level
            textBlock.FontSize = level switch
            {
                1 => 28,
                2 => 24,
                3 => 20,
                4 => 18,
                5 => 16,
                _ => 14
            };

            return textBlock;
        }

        private static StackPanel ParseList(string[] lines, ref int index)
        {
            var listPanel = new StackPanel { Spacing = 4, Margin = new Thickness(16, 4, 0, 4) };

            while (index < lines.Length)
            {
                var line = lines[index].TrimEnd('\r');
                var trimmed = line.TrimStart();

                if (!trimmed.StartsWith("- ") && !trimmed.StartsWith("* "))
                    break;

                var text = trimmed.Substring(2).Trim();
                var itemPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };

                itemPanel.Children.Add(new TextBlock
                {
                    Text = "•",
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Top
                });

                itemPanel.Children.Add(ParseInlineFormatting(text));

                listPanel.Children.Add(itemPanel);
                index++;
            }

            return listPanel;
        }

        private static StackPanel ParseNumberedList(string[] lines, ref int index)
        {
            var listPanel = new StackPanel { Spacing = 4, Margin = new Thickness(16, 4, 0, 4) };
            int number = 1;

            while (index < lines.Length)
            {
                var line = lines[index].TrimEnd('\r');
                var trimmed = line.TrimStart();

                if (!Regex.IsMatch(trimmed, @"^\d+\.\s"))
                    break;

                var match = Regex.Match(trimmed, @"^\d+\.\s");
                var text = trimmed.Substring(match.Length).Trim();

                var itemPanel = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };

                itemPanel.Children.Add(new TextBlock
                {
                    Text = $"{number}.",
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Top,
                    MinWidth = 24
                });

                itemPanel.Children.Add(ParseInlineFormatting(text));

                listPanel.Children.Add(itemPanel);
                index++;
                number++;
            }

            return listPanel;
        }

        private static TextBlock ParseParagraph(string line)
        {
            return ParseInlineFormatting(line);
        }

        private static Border ParseBlockquote(string[] lines, ref int index)
        {
            var quoteLines = new List<string>();

            // Collect all consecutive blockquote lines
            while (index < lines.Length)
            {
                var line = lines[index].TrimEnd('\r');
                var trimmed = line.TrimStart();

                if (!trimmed.StartsWith(">"))
                    break;

                // Remove > and any space after it
                var content = trimmed.Substring(1).TrimStart();
                quoteLines.Add(content);
                index++;
            }

            // Combine lines
            var fullText = string.Join("\n", quoteLines);

            // Parse inline formatting
            var textBlock = ParseInlineFormatting(fullText);
            textBlock.TextWrapping = TextWrapping.Wrap;

            // Create styled border for blockquote
            var border = new Border
            {
                Child = textBlock,
                Background = GetBlockquoteBackground(),
                BorderBrush = GetBlockquoteBorder(),
                BorderThickness = new Thickness(4, 0, 0, 0), // Left border accent
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 8, 0, 8),
                CornerRadius = new CornerRadius(0, 4, 4, 0) // Rounded on right side only
            };

            return border;
        }

        private static Brush GetBlockquoteBackground()
        {
            try
            {
                if (Application.Current?.Resources != null && 
                    Application.Current.Resources.ContainsKey("LayerFillColorDefaultBrush"))
                {
                    var resource = Application.Current.Resources["LayerFillColorDefaultBrush"];
                    if (resource is Brush brush)
                        return brush;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkdownHelper: Error getting BlockquoteBackground: {ex.Message}");
            }
            return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(30, 128, 128, 128));
        }

        private static Brush GetBlockquoteBorder()
        {
            try
            {
                if (Application.Current?.Resources != null && 
                    Application.Current.Resources.ContainsKey("SystemAccentColor"))
                {
                    var resource = Application.Current.Resources["SystemAccentColor"];
                    if (resource is Windows.UI.Color color)
                        return new SolidColorBrush(color);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkdownHelper: Error getting BlockquoteBorder: {ex.Message}");
            }
            return new SolidColorBrush(Colors.CornflowerBlue);
        }

        private static Grid? ParseTable(string[] lines, ref int index)
        {
            var tableLines = new List<string>();
            var startIndex = index;

            // Collect all consecutive lines that look like table rows
            while (index < lines.Length)
            {
                var line = lines[index].TrimEnd('\r').Trim();
                
                // Empty line ends table
                if (string.IsNullOrWhiteSpace(line))
                    break;
                
                // Must contain | to be a table row
                if (!line.Contains("|"))
                    break;

                tableLines.Add(line);
                index++;
            }

            // Need at least 2 lines (header + separator)
            if (tableLines.Count < 2)
            {
                index = startIndex;
                return null;
            }

            // Check if second line is separator (contains dashes and pipes)
            var separatorLine = tableLines[1];
            if (!IsTableSeparator(separatorLine))
            {
                index = startIndex;
                return null;
            }

            // Parse header
            var headers = ParseTableRow(tableLines[0]);
            if (headers.Count == 0)
            {
                index = startIndex;
                return null;
            }

            // Parse data rows (skip header and separator)
            var dataRows = new List<List<string>>();
            for (int i = 2; i < tableLines.Count; i++)
            {
                var row = ParseTableRow(tableLines[i]);
                if (row.Count > 0)
                    dataRows.Add(row);
            }

            // Create table UI
            return CreateTableGrid(headers, dataRows);
        }

        private static bool IsTableSeparator(string line)
        {
            // Separator line contains only |, -, :, and whitespace
            var cleaned = line.Trim();
            if (!cleaned.Contains("|")) return false;
            
            foreach (char c in cleaned)
            {
                if (c != '|' && c != '-' && c != ':' && !char.IsWhiteSpace(c))
                    return false;
            }
            
            return cleaned.Contains("-");
        }

        private static List<string> ParseTableRow(string line)
        {
            var cells = new List<string>();
            var cleaned = line.Trim();
            
            // Remove leading/trailing pipes
            if (cleaned.StartsWith("|"))
                cleaned = cleaned.Substring(1);
            if (cleaned.EndsWith("|"))
                cleaned = cleaned.Substring(0, cleaned.Length - 1);
            
            // Split by pipe
            var parts = cleaned.Split('|');
            foreach (var part in parts)
            {
                cells.Add(part.Trim());
            }
            
            return cells;
        }

        private static Grid CreateTableGrid(List<string> headers, List<List<string>> dataRows)
        {
            var grid = new Grid
            {
                Margin = new Thickness(0, 8, 0, 8)
            };

            // Define columns
            for (int i = 0; i < headers.Count; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            // Define rows (header + data rows)
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Header
            foreach (var _ in dataRows)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Add header cells
            for (int col = 0; col < headers.Count; col++)
            {
                var cell = CreateTableCell(headers[col], isHeader: true);
                Grid.SetRow(cell, 0);
                Grid.SetColumn(cell, col);
                grid.Children.Add(cell);
            }

            // Add data cells
            for (int row = 0; row < dataRows.Count; row++)
            {
                var dataRow = dataRows[row];
                for (int col = 0; col < Math.Min(dataRow.Count, headers.Count); col++)
                {
                    var cell = CreateTableCell(dataRow[col], isHeader: false);
                    Grid.SetRow(cell, row + 1);
                    Grid.SetColumn(cell, col);
                    grid.Children.Add(cell);
                }
            }

            // Wrap in border with styling
            var border = new Border
            {
                Child = grid,
                BorderBrush = GetCodeBlockBorderBrush(),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(4)
            };

            // Return grid in container
            var container = new Grid();
            container.Children.Add(border);
            return container;
        }

        private static Border CreateTableCell(string content, bool isHeader)
        {
            var textBlock = ParseInlineFormatting(content);
            
            if (isHeader)
            {
                textBlock.FontWeight = FontWeights.Bold;
            }

            var border = new Border
            {
                Child = textBlock,
                Padding = new Thickness(8, 6, 8, 6),
                BorderBrush = GetCodeBlockBorderBrush(),
                BorderThickness = new Thickness(0, 0, 1, 1),
                Background = isHeader ? GetCodeBlockBrush() : null
            };

            return border;
        }

        private static TextBlock ParseInlineFormatting(string text)
        {
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                IsTextSelectionEnabled = true
            };

            // Simple inline formatting (bold, italic, code)
            // For now, just display as plain text
            // You can extend this to parse **bold**, *italic*, `code`, etc.

            var inlines = ParseInlineElements(text);
            foreach (var inline in inlines)
            {
                textBlock.Inlines.Add(inline);
            }

            return textBlock;
        }

        private static List<Inline> ParseInlineElements(string text)
        {
            var inlines = new List<Inline>();
            
            // Pattern for inline code: `code`
            var codePattern = @"`([^`]+)`";
            // Pattern for bold: **text** or __text__
            var boldPattern = @"\*\*(.+?)\*\*|__(.+?)__";
            // Pattern for italic: *text* or _text_
            var italicPattern = @"\*(.+?)\*|_(.+?)_";

            var lastIndex = 0;
            var matches = new List<(int start, int length, string type, string content)>();

            // Find all inline code matches
            foreach (Match match in Regex.Matches(text, codePattern))
            {
                matches.Add((match.Index, match.Length, "code", match.Groups[1].Value));
            }

            // Find all bold matches (excluding those inside code)
            foreach (Match match in Regex.Matches(text, boldPattern))
            {
                if (!IsInsideMatch(match.Index, matches))
                {
                    var content = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    matches.Add((match.Index, match.Length, "bold", content));
                }
            }

            // Find all italic matches (excluding those inside code or bold)
            foreach (Match match in Regex.Matches(text, italicPattern))
            {
                if (!IsInsideMatch(match.Index, matches) && !IsPartOfBold(match, text))
                {
                    var content = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    matches.Add((match.Index, match.Length, "italic", content));
                }
            }

            // Sort matches by position
            matches = matches.OrderBy(m => m.start).ToList();

            foreach (var match in matches)
            {
                // Add text before this match
                if (match.start > lastIndex)
                {
                    var plainText = text.Substring(lastIndex, match.start - lastIndex);
                    if (!string.IsNullOrEmpty(plainText))
                        inlines.Add(new Run { Text = plainText });
                }

                // Add formatted text
                switch (match.type)
                {
                    case "code":
                        inlines.Add(new Run
                        {
                            Text = match.content,
                            FontFamily = new FontFamily("Consolas"),
                            Foreground = GetCodeForegroundBrush()
                        });
                        break;
                    case "bold":
                        inlines.Add(new Run
                        {
                            Text = match.content,
                            FontWeight = FontWeights.Bold
                        });
                        break;
                    case "italic":
                        inlines.Add(new Run
                        {
                            Text = match.content,
                            FontStyle = Windows.UI.Text.FontStyle.Italic
                        });
                        break;
                }

                lastIndex = match.start + match.length;
            }

            // Add remaining text
            if (lastIndex < text.Length)
            {
                inlines.Add(new Run { Text = text.Substring(lastIndex) });
            }

            // If no formatting found, return the whole text
            if (inlines.Count == 0)
            {
                inlines.Add(new Run { Text = text });
            }

            return inlines;
        }

        private static bool IsInsideMatch(int index, List<(int start, int length, string type, string content)> matches)
        {
            return matches.Any(m => index >= m.start && index < m.start + m.length);
        }

        private static bool IsPartOfBold(Match italicMatch, string text)
        {
            // Check if this * or _ is part of ** or __
            var pos = italicMatch.Index;
            if (pos > 0 && (text[pos - 1] == '*' || text[pos - 1] == '_'))
                return true;
            if (pos + italicMatch.Length < text.Length && 
                (text[pos + italicMatch.Length] == '*' || text[pos + italicMatch.Length] == '_'))
                return true;
            return false;
        }

        private static Paragraph CreateHeaderParagraph(string line)
        {
            int level = 0;
            while (level < line.Length && line[level] == '#')
                level++;

            var text = line.Substring(level).Trim();

            var run = new Run { Text = text };
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(run);

            paragraph.FontWeight = FontWeights.Bold;
            paragraph.FontSize = level switch
            {
                1 => 28,
                2 => 24,
                3 => 20,
                4 => 18,
                5 => 16,
                _ => 14
            };
            paragraph.Margin = new Thickness(0, level == 1 ? 16 : 12, 0, 8);

            return paragraph;
        }

        private static Paragraph CreateParagraphWithInlineFormatting(string text)
        {
            var paragraph = new Paragraph();
            var inlines = ParseInlineElements(text);
            foreach (var inline in inlines)
            {
                paragraph.Inlines.Add(inline);
            }
            return paragraph;
        }

        private static Brush GetCodeBlockBrush()
        {
            try
            {
                if (Application.Current?.Resources != null && 
                    Application.Current.Resources.ContainsKey("LayerFillColorDefaultBrush"))
                {
                    var resource = Application.Current.Resources["LayerFillColorDefaultBrush"];
                    if (resource is Brush brush)
                        return brush;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkdownHelper: Error getting CodeBlockBrush: {ex.Message}");
            }
            return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 45, 45, 45));
        }

        private static Brush GetCodeBlockBorderBrush()
        {
            try
            {
                if (Application.Current?.Resources != null && 
                    Application.Current.Resources.ContainsKey("CardStrokeColorDefaultBrush"))
                {
                    var resource = Application.Current.Resources["CardStrokeColorDefaultBrush"];
                    if (resource is Brush brush)
                        return brush;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkdownHelper: Error getting CodeBlockBorderBrush: {ex.Message}");
            }
            return new SolidColorBrush(Colors.Gray);
        }

        private static Brush GetCodeForegroundBrush()
        {
            try
            {
                if (Application.Current?.Resources != null && 
                    Application.Current.Resources.ContainsKey("SystemAccentColor"))
                {
                    var resource = Application.Current.Resources["SystemAccentColor"];
                    if (resource is Windows.UI.Color color)
                        return new SolidColorBrush(color);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MarkdownHelper: Error getting CodeForegroundBrush: {ex.Message}");
            }
            return new SolidColorBrush(Colors.DeepSkyBlue);
        }
    }
}
