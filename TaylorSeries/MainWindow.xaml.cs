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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();

            BisectionUserControl.SnackBarNotif = this.Snackbar_Main;
            SecantUserControl   .SnackBarNotif = this.Snackbar_Main;
            NewtonsUserControl  .SnackBarNotif = this.Snackbar_Main;
            

        }

    }

    public class BisectionResultRow {

        public enum FormulaSymbols { A, B }

        public Symbol Symbol             { private set;  get; }
        public Expr   FunctionExpression { private set;  get; }

        //variables
        public int    Iteration { set; get; }
        public double Epsillon  { set; get; }
        public double A         { set; get; }
        public double B         { set; get; }

        public double C   => (A + B) / 2;
        public double Abs => Math.Abs(A - B);

        public decimal GetAprox      { set; get; }
        public int     DecimalPlaces => UIHelper.CountDigitsAfterDecimal(Epsillon.ToString());

        public double FunctOfA {
            get {
                Dictionary<string, FloatingPoint> list = new Dictionary<string, FloatingPoint>();
                list.Add(Symbol.Item, A);

                return Evaluate.Evaluate(list, FunctionExpression).RealValue;
            }
        }

        public double FunctOfB {
            get {
                Dictionary<string, FloatingPoint> list = new Dictionary<string, FloatingPoint>();
                list.Add(Symbol.Item, B);

                return Evaluate.Evaluate(list, FunctionExpression).RealValue;
            }
        }

        public double FunctOfC {
            get {
                Dictionary<string, FloatingPoint> list = new Dictionary<string, FloatingPoint>();
                list.Add(Symbol.Item, C);

                return Evaluate.Evaluate(list, FunctionExpression).RealValue;
            }
        }

        public bool ISAccurateEnough => Abs <= Epsillon;
        public bool IsAPositive      => FunctOfA >= 0;
        public bool IsBPositive      => FunctOfB >= 0;
        public bool IsCPositive      => FunctOfC >= 0;

        public bool IsANegative      => !IsAPositive;
        public bool IsBNegative      => !IsBPositive;
        public bool IsCNegative      => !IsCPositive;

        public BisectionResultRow(Symbol symbol, Expr functionExpression) {
            this.FunctionExpression = functionExpression;
            this.Symbol             = symbol;
        }

        public FormulaSymbols GetPositiveSymbol() {
            return (IsAPositive) ? FormulaSymbols.A : FormulaSymbols.B;
        }

        public FormulaSymbols GetNegativeSymbol() {
            return (IsANegative) ? FormulaSymbols.A : FormulaSymbols.B;
        }

        public BisectionResultRow NextIteration() {
            BisectionResultRow Next = new BisectionResultRow(this.Symbol, this.FunctionExpression) {
                Iteration = this.Iteration + 1,
                Epsillon  = this.Epsillon     
            };

            if (IsCPositive) {
                switch (GetPositiveSymbol()) {
                    case FormulaSymbols.A:
                        Next.A = this.C;
                        Next.B = this.B;
                        break;
                    case FormulaSymbols.B:
                        Next.A = this.A;
                        Next.B = this.C;
                        break;
                }

            } else {
                switch (GetNegativeSymbol()) {
                    case FormulaSymbols.A:
                        Next.A = this.C;
                        Next.B = this.B;
                        break;
                    case FormulaSymbols.B:
                        Next.A = this.A;
                        Next.B = this.C;
                        break;
                }
            }
            return Next;
        }

        public static List<BisectionResultRow> ComputeList(BisectionResultRow InitialRow) {
            //initiialize
            BisectionResultRow Row = InitialRow;
            List<BisectionResultRow> List = new List<BisectionResultRow>();

            //loop until exceed epsillon
            while(!Row.ISAccurateEnough) {
                List.Add(Row);
                Row = Row.NextIteration();
            }

            return List;
        }

        override
        public string ToString() {
            return $"{Iteration+1} \t {A} \t {B} \t {FunctOfA} \t {FunctOfB} \t {C} \t {Abs} \t {ISAccurateEnough} \t {FunctOfC}";
        }
    }

    public class NewtonsMethod {

    }

    public class SecantMethodDecimal {
        //used for storing each row and output to listview
        public class SecantResultRow {
            public int Index { set; get; }
            public decimal Value { set; get; }
            public decimal Accuracy { set; get; }
        }

        //holds all the results
        public Dictionary<int, decimal> ResultDictionary = new Dictionary<int, decimal>();

        //get a list ver. of the result dict.
        public List<SecantResultRow> Results {
            get
            {
                List<SecantResultRow> results = new List<SecantResultRow>();

                foreach (KeyValuePair<int, decimal> item in ResultDictionary)
                {
                    SecantResultRow row = new SecantResultRow() {
                        Index = item.Key,
                        Value = item.Value,

                        Accuracy = Math.Abs((item.Key == 1) ? 0 : item.Value - ResultDictionary[item.Key - 1])
                    };

                    results.Add(row);
                }
                return results;
            }
        }

        public decimal Epsillon  { set; get; }
        public Expr    Function  { set; get; }
        public string  SymbolStr { set; get; }

        public decimal GetAprox      => ResultDictionary.Last().Value;
        public int     DecimalPlaces => UIHelper.CountDigitsAfterDecimal(Epsillon.ToString());
        public decimal GetRoundedOff => Math.Round(GetAprox, DecimalPlaces);
        public decimal GetChoppedOff => UIHelper.TruncateDecimal(GetAprox, DecimalPlaces);

        private decimal ComputeFormula(decimal a, decimal b, decimal c, decimal d) {
            return a - (b / ((b - c) / d));
        }

        private decimal EvaluateFormula(decimal value) {
            Dictionary<string, FloatingPoint> symbols = new Dictionary<string, FloatingPoint>();
            symbols.Add(SymbolStr, (double)value);
            return (decimal)Evaluate.Evaluate(symbols, Function).RealValue;
        }

        private decimal ComputeIteration(int i) {
            decimal a, b, c, d;

            a = ResultDictionary[i];
            b = EvaluateFormula(ResultDictionary[i]);
            c = EvaluateFormula(ResultDictionary[i - 1]);
            d = ResultDictionary[i] - ResultDictionary[i - 1];

            return ComputeFormula(a, b, c, d);
        }

        public void ComputeSecantMethod(decimal x1, decimal x2) {
            int iteration = 1;
            decimal result = 0;
            decimal accuracy = 0;

            //init.
            ResultDictionary.Add(iteration++, x1); //1
            ResultDictionary.Add(iteration, x2); //2

            do
            {
                result = ComputeIteration(iteration);
                accuracy = Math.Abs(result - ResultDictionary[iteration]);
                ResultDictionary.Add(++iteration, result);
            } while (accuracy > Epsillon);

        }


    }

    public class NewtonsMethodDecimal {
        //used for showing the sol.
        public class NewtonsResultRow {
            public int     Iteration { set; get; }
            public decimal Value     { set; get; }
            public decimal Accuracy  { set; get; }
        }

        //stores the results of each iteration
        public Dictionary<int, decimal> Values = new Dictionary<int, decimal>();

        //properties that need to be init
        public decimal Epsillon  { set; get; }
        public Expr    Function  { set; get; }
        public string  SymbolStr { set; get; }

        //get the derivative of the funct
        public Expr FunctDerivative {
            get {
                var symbol = Expr.Symbol(SymbolStr);
                return Calculus.Differentiate(symbol, Function);
            }
        }

        public decimal GetAprox      => Values.Last().Value;
        public int     DecimalPlaces => UIHelper.CountDigitsAfterDecimal(Epsillon.ToString());
        public decimal GetRoundedOff => Math.Round(GetAprox, DecimalPlaces);
        public decimal GetChoppedOff => UIHelper.TruncateDecimal(GetAprox, DecimalPlaces);

        //get a list ver. of the result dict.
        public List<NewtonsResultRow> Results {
            get {
                List<NewtonsResultRow> results = new List<NewtonsResultRow>();

                foreach (KeyValuePair<int, decimal> item in Values) {
                    NewtonsResultRow row = new NewtonsResultRow() {
                        Iteration = item.Key,
                        Value     = item.Value,

                        Accuracy = Math.Abs((item.Key == 1) ? 0 : item.Value - Values[item.Key - 1])
                    };

                    results.Add(row);
                }
                return results;
            }
        }

        

        private decimal EvaluateFunction(decimal value) {
            Dictionary<string, FloatingPoint> symbols = new Dictionary<string, FloatingPoint>();
            symbols.Add(SymbolStr, (double)value);
            return (decimal)Evaluate.Evaluate(symbols, Function).RealValue;
        }

        private decimal EvaluateFunctDerivative(decimal value) {
            Dictionary<string, FloatingPoint> symbols = new Dictionary<string, FloatingPoint>();
            symbols.Add(SymbolStr, (double)value);
            return (decimal)Evaluate.Evaluate(symbols, FunctDerivative).RealValue;
        }

        private decimal ComputeFormula(decimal xn) {
            return xn - (EvaluateFunction(xn)/EvaluateFunctDerivative(xn));
        }

        public void ComputeNewton(decimal x1) {
            //init
            int iteration = 1;
            Values.Add(iteration, x1);
            decimal accuracy = 1, result;

            do {
                result = ComputeFormula(Values[iteration]);
                Values.Add(iteration + 1, result);
                accuracy = Math.Abs(result - Values[iteration]);
                iteration++;
            } while (accuracy > Epsillon);

        }



    }

    public class TaylorSeriesIteration {
        public string Iteration  { set; get; }
        public string Evaluation { set; get; }

        public Expr Function { set; get; }
        public Expr Series   { set; get; }

        //property for getting Func Expression as Latex string
        public string FunctionAsLatexString {
            get {
                return LaTeX.Format(Function);
            }
        }

        //property for getting Func Expression as Infix string
        public string FunctionAsInfixString {
            get {
                return Infix.Format(Function);
            }
        }

        //property for getting Expr Expression as Latex string
        public string SeriesAsLatexString {
            get {
                return LaTeX.Format(Series);
            }
        }

        //property for getting Expr Expression as Infix string
        public string SeriesAsInfixString {
            get {
                return Infix.Format(Series);
            }
        }
    }

    public class TaylorSeriesCompute {
        //list for store each iteration of TS
        public List<TaylorSeriesIteration> DerivativeList { set; get; }

        public Expr SeriesExpression  { set; get; }
        public Expr InputFunction     { set; get; }
        public Expr FunctionInputExpr { set; get; }

        public FloatingPoint Aprox      { set; get; }
        public int           Iterations { set; get; }


        //property for getting input Expression as Latex string
        public string InputFunctionAsLatexString {
            get {
                return LaTeX.Format(InputFunction);
            }
        }
        //property for getting input Expression as Infix string
        public string InputFunctionAsInfixString {
            get {
                return Infix.Format(InputFunction);
            }
        }

        //property for getting series Expression as Latex string
        public string SeriesAsLatexString {
            get {
                return LaTeX.Format(SeriesExpression);
            }
        }
        //property for getting series Expression as Infix string
        public string SeriesAsInfixString {
            get {
                return Infix.Format(SeriesExpression);
            }
        }


        //create the function
        Expr Taylor(int iterations, Expr symbol, Expr value, Expr function) {
            this.DerivativeList.Clear();

            //counter for factorial
            int factorial = 1;

            //accumulates the results for each iteration (formula)
            Expr accumulator = Expr.Zero;

            //variable for holding the derivative of function for each iteration
            Expr derivative = function;

            for (int i = 0; i < iterations; i++) {
                //use for storing output
                TaylorSeriesIteration OutputItem = new TaylorSeriesIteration();

                //store the current iteration
                OutputItem.Iteration = (i + 1).ToString();

                //subs/replaces symbol with value from funct. Ex. symbol: x, func: x + 1, value: 1 result: 1 + 1 
                var subs = Structure.Substitute(symbol, value, derivative);

                //get the derivative of derivative with respect to symbol
                derivative = Calculus.Differentiate(symbol, derivative);

                //output current derivative
                OutputItem.Function = derivative;

                //evaluate derivative, f(0)
                var evalValue = new Dictionary<string, FloatingPoint> {
                    { Infix.Format(symbol), 1 }
                };
                var eval = Evaluate.Evaluate(evalValue, Structure.Substitute(symbol, 0, derivative));
                OutputItem.Evaluation = eval.ToString();

                //create the formula and append to accumulator
                accumulator = accumulator + subs / factorial * Expr.Pow(symbol - value, i);

                //output current formula
                OutputItem.Series = accumulator;

                //current iteration + 1 as factorial (cause 0-based loop)
                factorial *= (i + 1);

                //append to list
                this.DerivativeList.Add(OutputItem);
            }

            //return the formula/func in expanded form
            return Algebraic.Expand(accumulator);
        }

        public TaylorSeriesCompute(int iterations, Expr symbol, Expr value, Expr function, Expr functionInput) {
            //initiliaze
            DerivativeList = new List<TaylorSeriesIteration>();

            //create the series
            var series = Taylor(iterations, symbol, value, function);

            //eval
            var symbols = new Dictionary<string, FloatingPoint>();
            symbols.Add("x", 1);
            try {
                var eval = Evaluate.Evaluate(symbols, functionInput);
                //assign values
                this.Aprox = eval;

            } catch {
                throw new ArgumentException("Invalid function input");
            }

            //assign to results
            this.SeriesExpression = series;
            this.InputFunction = function;

            //var X = Expr.Symbol("x");
            //var sample = Taylor(6, X, 0, Expr.Cos(X));

            //string res = Infix.Format(sample);
            //Console.WriteLine("taylor series: {0}", res);
        }
    }

    public static class UIHelper {
        //used to create a snackbar message
        public static UIElement NewSnackbarMessage(PackIconKind iconkind, string message) {
            //create the stackpanel
            var sp_parent = new StackPanel() {
                Orientation = Orientation.Horizontal
            };

            //create the icon
            var icon = new PackIcon() {
                Kind = iconkind,
                Width = 25,
                Height = 25,
                Foreground = new SolidColorBrush(Colors.White),
                RenderTransformOrigin = new Point(0.5, 0.5),
            };

            //create textblock
            var label = new TextBlock() {
                Text = message,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White)
            };

            //add to the stackpanel
            sp_parent.Children.Add(icon);
            sp_parent.Children.Add(label);

            return sp_parent;
        }

        //Snackbar Message that blinks 
        public static UIElement NewSnackbarMessage(PackIconKind iconkind, string message, Color blinkColor, double speed = 1) {
            //create the color animation
            var ca = new ColorAnimation() {
                From = Colors.White,
                To = blinkColor,
                Duration = new Duration(TimeSpan.FromSeconds(speed)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase() { EasingMode = EasingMode.EaseInOut }
            };

            //new snakcbar message
            var SnackbarMessage = UIHelper.NewSnackbarMessage(iconkind, message) as StackPanel;
            //get elements
            var icon = SnackbarMessage.Children.OfType<PackIcon>().First();
            var label = SnackbarMessage.Children.OfType<TextBlock>().First();

            //animate elements
            icon.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ca);
            label.Foreground.BeginAnimation(SolidColorBrush.ColorProperty, ca);

            return SnackbarMessage;
        }

        public static void ResizeListViewColumnsToContent(object sender, RoutedEventArgs e) {
            var listView = (ListView)sender;
            var gridView = listView.View as GridView;
            if (gridView != null)
            {
                foreach (var column in gridView.Columns)
                {
                    if (double.IsNaN(column.Width))
                    {
                        column.Width = column.ActualWidth;
                    }
                    column.Width = double.NaN;
                }
            }
        }

        public static int CountDigitsAfterDecimal(double value) {
            bool start = false;
            int count = 0;
            foreach (var s in value.ToString())
            {
                if (s == '.')
                {
                    start = true;
                }
                else if (start)
                {
                    count++;
                }
            }

            return count;
        }

        public static int CountDigitsAfterDecimal(string value) {
            bool start = false;
            int count = 0;
            foreach (var s in value)
            {
                if (s == '.')
                {
                    start = true;
                }
                else if (start)
                {
                    count++;
                }
            }

            return count;
        }

        public static decimal TruncateDecimal(this decimal value, int decimalPlaces) {
            decimal integralValue = Math.Truncate(value);

            decimal fraction = value - integralValue;

            decimal factor = (decimal)Math.Pow(10, decimalPlaces);

            decimal truncatedFraction = Math.Truncate(fraction * factor) / factor;

            decimal result = integralValue + truncatedFraction;

            return result;
        }
    }

}
