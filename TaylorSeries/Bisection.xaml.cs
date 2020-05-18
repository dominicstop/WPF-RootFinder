using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MaterialDesignThemes.Wpf;
using System.Windows.Media.Animation;
using MathNet.Symbolics;
using Expr = MathNet.Symbolics.Expression;

namespace RootFinding {
    /// <summary>
    /// Interaction logic for Bisection.xaml
    /// </summary>
    public partial class Bisection : UserControl {
        public Snackbar SnackBarNotif { set; get; }

        public Bisection() {
            InitializeComponent();
            //Button_EvaluateExpression_Click(Button_EvaluateExpression, null);
        }

        private void Button_CheckExpression_Click(object sender, RoutedEventArgs e) {
            FC_InputEvaluation.Formula = TextBox_ExpressionInput.Text;
        }

        private void Button_EvaluateExpression_Click(object sender, RoutedEventArgs e) {
            try {
                Expr expression = Infix.ParseOrThrow(TextBox_ExpressionInput.Text);
                Symbol symbol   = Symbol.NewSymbol(TextBox_InputSymbol.Text);

                var result = new BisectionResultRow(symbol, expression) {
                    A = Double.Parse(TextBox_InputA.Text),
                    B = Double.Parse(TextBox_InputB.Text),
                    Epsillon = Double.Parse(TextBox_InputEpsilon.Text)
                };

                var results = BisectionResultRow.ComputeList(result);
                this.ListView_Solution.ItemsSource = results;
                UIHelper.ResizeListViewColumnsToContent(this.ListView_Solution, null);

                var lastResult    = results.Last();
                int decimalPlaces = UIHelper.CountDigitsAfterDecimal(TextBox_InputEpsilon.Text); 


                this.FC_Aproximation.Formula        = lastResult.B.ToString();
                this.FC_AproximationRounded.Formula = Math.Round(lastResult.B, decimalPlaces).ToString();
                this.FC_AproximationChopped.Formula = UIHelper.TruncateDecimal((decimal)lastResult.B, decimalPlaces).ToString();

                SnackBarNotif.MessageQueue.Enqueue(
                    UIHelper.NewSnackbarMessage(
                        PackIconKind.KeyboardVariant,
                        "Success! Check solution and results!",
                        Colors.DarkGreen,
                        0.5
                    )
                );
            }
            catch {
                //show error message
                SnackBarNotif.MessageQueue.Enqueue(
                    UIHelper.NewSnackbarMessage(
                        PackIconKind.KeyboardVariant,
                        "Invalid Input! Function Input (Value to Aprox.) syntax is invalid!",
                        Colors.HotPink,
                        0.5
                    )
                );
            }

        }
    }
}
