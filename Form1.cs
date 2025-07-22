namespace posicion_falsa;
using NCalc;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private double EvaluarFuncion(string funcion, double x)
    {
        try
        {
            // Procesar la funci�n para hacerla compatible con NCalc
            string funcionProcesada = ProcesarFuncion(funcion);

            var expression = new Expression(funcionProcesada);
            expression.Parameters["x"] = x;
            expression.Parameters["X"] = x;

            // Agregar funciones matem�ticas personalizadas
            expression.EvaluateFunction += (name, args) =>
            {
                switch (name.ToLower())
                {
                    case "pow":
                        if (args.Parameters.Length == 2)
                        {
                            double baseVal = Convert.ToDouble(args.Parameters[0].Evaluate());
                            double expVal = Convert.ToDouble(args.Parameters[1].Evaluate());
                            args.Result = Math.Pow(baseVal, expVal);
                        }
                        break;
                    case "sin":
                        if (args.Parameters.Length == 1)
                        {
                            double val = Convert.ToDouble(args.Parameters[0].Evaluate());
                            args.Result = Math.Sin(val);
                        }
                        break;
                    case "cos":
                        if (args.Parameters.Length == 1)
                        {
                            double val = Convert.ToDouble(args.Parameters[0].Evaluate());
                            args.Result = Math.Cos(val);
                        }
                        break;
                    case "tan":
                        if (args.Parameters.Length == 1)
                        {
                            double val = Convert.ToDouble(args.Parameters[0].Evaluate());
                            args.Result = Math.Tan(val);
                        }
                        break;
                    case "exp":
                        if (args.Parameters.Length == 1)
                        {
                            double val = Convert.ToDouble(args.Parameters[0].Evaluate());
                            args.Result = Math.Exp(val);
                        }
                        break;
                    case "log":
                        if (args.Parameters.Length == 1)
                        {
                            double val = Convert.ToDouble(args.Parameters[0].Evaluate());
                            args.Result = Math.Log(val);
                        }
                        break;
                    case "sqrt":
                        if (args.Parameters.Length == 1)
                        {
                            double val = Convert.ToDouble(args.Parameters[0].Evaluate());
                            args.Result = Math.Sqrt(val);
                        }
                        break;
                }
            };

            return Convert.ToDouble(expression.Evaluate());
        }
        catch (Exception)
        {
            throw new Exception($"Error al evaluar la funci�n: {funcion}");
        }
    }

    private string ProcesarFuncion(string funcion)
    {
        // Convertir a min�sculas para procesamiento
        string funcionProcesada = funcion.ToLower();

        // Reemplazos comunes para hacer compatible con NCalc
        funcionProcesada = funcionProcesada.Replace(" ", ""); // Quitar espacios

        // Mantener pow() como est�, ya que lo manejamos con EvaluateFunction
        // Convertir otras funciones matem�ticas comunes
        funcionProcesada = funcionProcesada.Replace("sen(", "sin(");
        funcionProcesada = funcionProcesada.Replace("ln(", "log(");

        // Agregar multiplicaci�n impl�cita donde sea necesario
        funcionProcesada = AgregarMultiplicacionImplicita(funcionProcesada);

        return funcionProcesada;
    }

    private string AgregarMultiplicacionImplicita(string funcion)
    {
        string resultado = "";
        for (int i = 0; i < funcion.Length; i++)
        {
            resultado += funcion[i];

            // Si el car�cter actual es un d�gito y el siguiente es 'x' o '('
            if (i < funcion.Length - 1)
            {
                char actual = funcion[i];
                char siguiente = funcion[i + 1];

                if ((char.IsDigit(actual) && siguiente == 'x') ||
                    (char.IsDigit(actual) && siguiente == '(') ||
                    (actual == 'x' && siguiente == '(') ||
                    (actual == ')' && siguiente == '(') ||
                    (actual == ')' && siguiente == 'x') ||
                    (actual == 'x' && char.IsDigit(siguiente)))
                {
                    resultado += "*";
                }
            }
        }
        return resultado;
    }

    private void btnCalcular_Click(object sender, EventArgs e)
    {
        try
        {
            string funcion = txtFuncion.Text.Trim();

            if (string.IsNullOrEmpty(funcion))
            {
                MessageBox.Show("Por favor ingrese una funci�n.");
                return;
            }

            double a = double.Parse(txtA.Text);
            double b = double.Parse(txtB.Text);
            double tol = double.Parse(txtTolerancia.Text);
            int maxIter = int.Parse(txtMaxIteraciones.Text);

            // Verificar que el intervalo sea v�lido
            if (a >= b)
            {
                MessageBox.Show("El valor de 'a' debe ser menor que 'b'.");
                return;
            }

            double fa = EvaluarFuncion(funcion, a);
            double fb = EvaluarFuncion(funcion, b);

            if (fa * fb >= 0)
            {
                MessageBox.Show($"La funci�n debe cambiar de signo entre a y b.\nf({a}) = {fa:F6}\nf({b}) = {fb:F6}");
                return;
            }

            string resultado = "";
            double c = a;
            double fc;
            int iter = 0;
            double error;

            // Encabezado
            resultado += "M�TODO DE LA POSICI�N FALSA (REGLA FALSI)\n";
            resultado += new string('=', 50) + "\n";
            resultado += $"Funci�n: {funcion}\n";
            resultado += $"Intervalo inicial: [{a}, {b}]\n";
            resultado += $"f({a}) = {fa:F6}, f({b}) = {fb:F6}\n";
            resultado += $"Tolerancia: {tol}\n";
            resultado += $"M�ximo de iteraciones: {maxIter}\n\n";

            do
            {
                fa = EvaluarFuncion(funcion, a);
                fb = EvaluarFuncion(funcion, b);

                // F�rmula del m�todo de la posici�n falsa
                c = (a * fb - b * fa) / (fb - fa);
                fc = EvaluarFuncion(funcion, c);
                error = Math.Abs(fc);

                resultado += $"Iteraci�n {iter + 1}:\n";
                resultado += $"  c = ({a:F6} � {fb:F6} - {b:F6} � {fa:F6}) / ({fb:F6} - {fa:F6})\n";
                resultado += $"  c = {c:F6}\n";
                resultado += $"  f(c) = f({c:F6}) = {fc:F6}\n";
                resultado += $"  Error = |f(c)| = {error:F6}\n";

                // Determinar el nuevo intervalo
                if (fa * fc < 0)
                {
                    resultado += $"  f(a)�f(c) < 0, nuevo intervalo: [{a:F6}, {c:F6}]\n";
                    b = c;
                }
                else
                {
                    resultado += $"  f(a)�f(c) > 0, nuevo intervalo: [{c:F6}, {b:F6}]\n";
                    a = c;
                }

                resultado += "\n";
                iter++;

                if (iter >= maxIter)
                {
                    resultado += $"Se alcanz� el m�ximo n�mero de iteraciones ({maxIter}).\n";
                    break;
                }

            } while (error > tol);

            if (error <= tol)
            {
                resultado += "CONVERGENCIA ALCANZADA\n";
                resultado += new string('-', 30) + "\n";
                resultado += $"Ra�z aproximada: {c:F8}\n";
                resultado += $"Error final: {error:F8}\n";
                resultado += $"N�mero de iteraciones: {iter}\n";
                resultado += $"Verificaci�n: f({c:F8}) = {fc:F8}";
            }

            txtResultado.Text = resultado;
        }
        catch (Exception ex)
        {
            string mensaje = "Error: " + ex.Message + "\n\n";
            mensaje += "Ejemplos de funciones v�lidas:\n";
            mensaje += "� Polinomios: x^2 - 4, x^3 - 2*x - 1\n";
            mensaje += "� Con pow(): pow(x,2) - 4, pow(x,3) - 2*x - 1\n";
            mensaje += "� Trigonom�tricas: sin(x) - 0.5, cos(x) - x\n";
            mensaje += "� Exponenciales: exp(x) - 2, log(x) - 1\n";
            mensaje += "� Mixtas: x^2 + sin(x) - 1\n\n";
            mensaje += "Nota: Use 'x' como variable independiente";

            MessageBox.Show(mensaje);
        }
    }
}
