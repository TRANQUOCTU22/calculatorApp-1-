#nullable disable // Tắt cảnh báo null của phiên bản mới
using System;
using System.Collections.Generic;
using System.Diagnostics; // Thư viện đo thời gian (Stopwatch)
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// Tên namespace theo ảnh bạn gửi là 'calculatorApp'
namespace calculatorApp
{
    // ==========================================
    // PHẦN 1: CẤU TRÚC DỮ LIỆU TỰ ĐỊNH NGHĨA (Theo yêu cầu GV)
    // ==========================================

    // Class Node: Đại diện cho 1 phần tử trong Stack
    public class Node<T>
    {
        public T Data;
        public Node<T> Next;

        public Node(T data)
        {
            Data = data;
            Next = null;
        }
    }

    // Class MyStack: Tự viết lại Stack (Không dùng Stack có sẵn của C#)
    public class MyStack<T>
    {
        private Node<T> top;
        private int count;

        public MyStack()
        {
            top = null;
            count = 0;
        }

        public bool IsEmpty()
        {
            return top == null;
        }

        public void Push(T data)
        {
            Node<T> newNode = new Node<T>(data);
            newNode.Next = top;
            top = newNode;
            count++;
        }

        public T Pop()
        {
            if (IsEmpty()) throw new Exception("Stack rỗng (Underflow)!");
            T data = top.Data;
            top = top.Next;
            count--;
            return data;
        }

        public T Peek()
        {
            if (IsEmpty()) throw new Exception("Stack rỗng!");
            return top.Data;
        }

        public int Count => count;
    }

    // ==========================================
    // PHẦN 2: ENGINE TÍNH TOÁN & THUẬT TOÁN
    // ==========================================
    public class CalculatorEngine
    {
        // Biến lưu thời gian thực thi để báo cáo hiệu năng
        public long ExecutionTime { get; private set; }

        // Thuật toán 1: Xác định độ ưu tiên toán tử
        private int GetPrecedence(string op)
        {
            if (op == "*" || op == "/") return 2;
            if (op == "+" || op == "-") return 1;
            return 0;
        }

        // Thuật toán 2: Kiểm tra tính hợp lệ của dấu ngoặc (Validate Parentheses)
        // Đây là thuật toán bổ sung để báo cáo của bạn dày hơn
        public bool IsValidExpression(string expression)
        {
            MyStack<char> checkStack = new MyStack<char>();
            foreach (char c in expression)
            {
                if (c == '(') checkStack.Push(c);
                else if (c == ')')
                {
                    if (checkStack.IsEmpty()) return false;
                    checkStack.Pop();
                }
            }
            return checkStack.IsEmpty();
        }

        // Thuật toán 3: Shunting-yard (Biến đổi Trung tố -> Hậu tố)
        public MyStack<string> InfixToPostfix(string expression)
        {
            MyStack<string> outputQueue = new MyStack<string>(); // Dùng Stack làm nơi chứa tạm kết quả đảo ngược
            MyStack<string> outputFinal = new MyStack<string>(); // Kết quả cuối cùng
            MyStack<string> opStack = new MyStack<string>();

            string pattern = @"(\d+(\.\d+)?)|([+\-*/()])";

            foreach (Match match in Regex.Matches(expression, pattern))
            {
                string token = match.Value;

                if (double.TryParse(token, out _)) // Là số
                {
                    outputQueue.Push(token);
                }
                else if (token == "(")
                {
                    opStack.Push(token);
                }
                else if (token == ")")
                {
                    while (!opStack.IsEmpty() && opStack.Peek() != "(")
                    {
                        outputQueue.Push(opStack.Pop());
                    }
                    if (!opStack.IsEmpty()) opStack.Pop(); // Bỏ dấu "("
                }
                else // Là toán tử
                {
                    while (!opStack.IsEmpty() && GetPrecedence(opStack.Peek()) >= GetPrecedence(token))
                    {
                        outputQueue.Push(opStack.Pop());
                    }
                    opStack.Push(token);
                }
            }

            while (!opStack.IsEmpty())
            {
                outputQueue.Push(opStack.Pop());
            }

            // Vì outputQueue đang chứa ngược (do dùng cơ chế Push), ta cần đảo lại cho đúng thứ tự Postfix
            // Đây là Thuật toán 4: Đảo ngược Stack (Reverse Stack)
            while (!outputQueue.IsEmpty())
            {
                outputFinal.Push(outputQueue.Pop());
            }
            // Lưu ý: Lúc này outputFinal chứa dạng Postfix nhưng thứ tự lấy ra sẽ là từ trái sang phải
            // Để dễ xử lý trong hàm Evaluate, ta trả về List hoặc xử lý đảo ngược lần nữa. 
            // Ở đây để đơn giản và đúng logic MyStack, tôi sẽ trả về outputFinal và xử lý pop.

            return outputFinal;
        }

        // Thuật toán 5: Tính toán giá trị biểu thức Hậu tố (Postfix Evaluation)
        public double EvaluatePostfix(MyStack<string> postfixReverse)
        {
            // postfixReverse đang chứa: Top -> [3, 4, +, ...] -> Bottom
            // Ta cần lấy ra lần lượt. 
            // Tuy nhiên để thuận tiện, ta chuyển sang List để duyệt xuôi
            List<string> tokens = new List<string>();
            while (!postfixReverse.IsEmpty())
            {
                tokens.Add(postfixReverse.Pop());
            }
            // Tokens giờ là: 3, 4, + (đúng thứ tự)

            MyStack<double> stack = new MyStack<double>();

            foreach (string token in tokens)
            {
                if (double.TryParse(token, out double number))
                {
                    stack.Push(number);
                }
                else
                {
                    if (stack.Count < 2) throw new ArgumentException("Lỗi cú pháp toán học");

                    double val2 = stack.Pop();
                    double val1 = stack.Pop();

                    switch (token)
                    {
                        case "+": stack.Push(val1 + val2); break;
                        case "-": stack.Push(val1 - val2); break;
                        case "*": stack.Push(val1 * val2); break;
                        case "/":
                            if (val2 == 0) throw new DivideByZeroException();
                            stack.Push(val1 / val2);
                            break;
                    }
                }
            }
            return stack.Count > 0 ? stack.Pop() : 0;
        }

        // Hàm Wrapper tính toán tổng hợp (Có đo thời gian)
        public double Calculate(string expression)
        {
            // Bắt đầu đo giờ
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // 1. Kiểm tra ngoặc
            if (!IsValidExpression(expression))
                throw new Exception("Lỗi: Dấu ngoặc không hợp lệ!");

            // 2. Chuyển đổi
            var postfix = InfixToPostfix(expression);

            // 3. Tính toán
            double result = EvaluatePostfix(postfix);

            // Kết thúc đo giờ
            sw.Stop();
            ExecutionTime = sw.ElapsedTicks; // Lưu lại số Ticks

            return result;
        }
    }

    // ==========================================
    // PHẦN 3: GIAO DIỆN NGƯỜI DÙNG (UI)
    // ==========================================
    public partial class Form1 : Form
    {
        private TextBox txtDisplay;
        private Label lblPreview;
        private CalculatorEngine engine = new CalculatorEngine();

        // Cấu hình màu sắc (Dark Mode đẹp mắt)
        private Color colBackground = Color.FromArgb(32, 32, 32);
        private Color colButtonNum = Color.FromArgb(59, 59, 59);
        private Color colButtonOp = Color.FromArgb(255, 149, 0); // Màu cam iOS
        private Color colText = Color.White;

        public Form1()
        {
            // Bỏ qua InitializeComponent để tránh lỗi Designer khi copy code
            try { InitializeComponent(); } catch { }
            SetupUI();
        }

        private void SetupUI()
        {
            this.Text = "Đồ Án Cấu Trúc Dữ Liệu";
            this.Size = new Size(360, 550); // Form to hơn xíu để hiện thời gian
            this.BackColor = colBackground;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Controls.Clear();

            // Label hiển thị biểu thức và thời gian chạy
            lblPreview = new Label()
            {
                ForeColor = Color.LightGray,
                Font = new Font("Segoe UI", 9),
                TextAlign = ContentAlignment.MiddleRight,
                Location = new Point(10, 5),
                Size = new Size(320, 30),
                Text = "Ready"
            };
            this.Controls.Add(lblPreview);

            // Màn hình chính
            txtDisplay = new TextBox()
            {
                BackColor = colBackground,
                ForeColor = colText,
                Font = new Font("Segoe UI", 32, FontStyle.Bold),
                TextAlign = HorizontalAlignment.Right,
                BorderStyle = BorderStyle.None,
                Location = new Point(10, 40),
                Size = new Size(320, 60),
                ReadOnly = true,
                Text = "0"
            };
            this.Controls.Add(txtDisplay);

            // Danh sách nút
            string[,] buttons = {
                { "C", "(", ")", "/" },
                { "7", "8", "9", "*" },
                { "4", "5", "6", "-" },
                { "1", "2", "3", "+" },
                { "0", ".", "DEL", "=" }
            };

            int startX = 20, startY = 120, btnSize = 70, gap = 10;
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    string text = buttons[r, c];
                    Button btn = new Button()
                    {
                        Text = text,
                        Size = new Size(btnSize, btnSize),
                        Location = new Point(startX + c * (btnSize + gap), startY + r * (btnSize + gap)),
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 16, FontStyle.Bold),
                        ForeColor = colText
                    };
                    btn.FlatAppearance.BorderSize = 0;

                    if (text == "=") btn.BackColor = colButtonOp;
                    else if ("+-*/()".Contains(text)) btn.BackColor = colButtonOp;
                    else if (text == "C" || text == "DEL")
                    {
                        btn.BackColor = Color.FromArgb(165, 165, 165);
                        btn.ForeColor = Color.Black;
                    }
                    else btn.BackColor = colButtonNum;

                    // Bo tròn nút
                    GraphicsPath p = new GraphicsPath();
                    p.AddEllipse(0, 0, btnSize, btnSize);
                    btn.Region = new Region(p);

                    btn.Click += Button_Click;
                    this.Controls.Add(btn);
                }
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string text = btn.Text;
            string current = txtDisplay.Text;

            if (text == "=")
            {
                try
                {
                    // Gọi hàm tính toán
                    double result = engine.Calculate(current);

                    // Hiển thị kết quả
                    txtDisplay.Text = result.ToString();

                    // Hiển thị đánh giá hiệu năng (Time Complexity Measurement)
                    // Đây là phần ăn điểm với GV
                    lblPreview.Text = $"Time: {engine.ExecutionTime} ticks ({(engine.ExecutionTime / 10000.0):F4} ms)";
                }
                catch (DivideByZeroException)
                {
                    txtDisplay.Text = "Error";
                    MessageBox.Show("Không thể chia cho 0!");
                }
                catch (Exception ex)
                {
                    txtDisplay.Text = "Error";
                    MessageBox.Show(ex.Message);
                }
            }
            else if (text == "C")
            {
                txtDisplay.Text = "0";
                lblPreview.Text = "";
            }
            else if (text == "DEL")
            {
                txtDisplay.Text = current.Length > 1 ? current.Substring(0, current.Length - 1) : "0";
            }
            else
            {
                // Logic nhập số thông minh
                if (current == "0" || current == "Error") txtDisplay.Text = text;
                else txtDisplay.Text += text;
            }
        }
    }
}