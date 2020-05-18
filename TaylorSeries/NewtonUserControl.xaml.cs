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
    /// Interaction logic for NewtonUserControl.xaml
    /// </summary>
    public partial class NewtonUserControl : UserControl {
        public Snackbar SnackBarNotif { set; get; }

        public NewtonUserControl() {
            InitializeComponent();
        }

        private void Button_CheckExpression_Click(object sender, RoutedEventArgs e) {
            FC_InputEvaluation.Formula = TextBox_ExpressionInput.Text;
        }

        private void Button_EvaluateExpression_Click(object sender, RoutedEventArgs e) {
            try {
                Expr expression  = Infix.ParseOrThrow(TextBox_ExpressionInput.Text);
                string symbolstr = TextBox_InputSymbol.Text;

                string x1 = TextBox_InputX1     .Text;
                string ep = TextBox_InputEpsilon.Text;

                var result = new NewtonsMethodDecimal() {
                    Function  = expression,
                    SymbolStr = symbolstr ,
                    Epsillon  = Decimal.Parse(ep)
                };

                result.ComputeNewton(Decimal.Parse(x1));

                this.ListView_Solution.ItemsSource = result.Results;
                UIHelper.ResizeListViewColumnsToContent(this.ListView_Solution, null);

                this.FC_Aproximation.Formula        = result.GetAprox.ToString();
                this.FC_AproximationRounded.Formula = result.GetRoundedOff.ToString();
                this.FC_AproximationChopped.Formula = result.GetChoppedOff.ToString();            

                SnackBarNotif.MessageQueue.Enqueue(
                    UIHelper.NewSnackbarMessage(
                        PackIconKind.CheckAll,
                        "Success! Check solution and results!",
                        Colors.DarkGreen,
                        0.5
                    )
                );
            } catch {
                //show error message
                SnackBarNotif.MessageQueue.Enqueue(
                    UIHelper.NewSnackbarMessage(
                        PackIconKind.SignCaution,
                        "Error! Unable to compute aproximation!",
                        Colors.HotPink,
                        0.5
                    )
                );
            }
        }
    }
}
