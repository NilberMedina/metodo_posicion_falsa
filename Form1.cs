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
            // Procesar la función para hacerla compatible con NCalc
            string funcionProcesada = ProcesarFuncion(funcion);

            var expression = new Expression(funcionProcesada);
            expression.Parameters["x"] = x;
            expression.Parameters["X"] = x;

            // Agregar funciones matemáticas personalizadas
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
            throw new Exception($"Error al evaluar la función: {funcion}");
        }
    }

    private string ProcesarFuncion(string funcion)
    {
        // Convertir a minúsculas para procesamiento
        string funcionProcesada = funcion.ToLower();

        // Reemplazos comunes para hacer compatible con NCalc
        funcionProcesada = funcionProcesada.Replace(" ", ""); // Quitar espacios

        // Mantener pow() como está, ya que lo manejamos con EvaluateFunction
        // Convertir otras funciones matemáticas comunes
        funcionProcesada = funcionProcesada.Replace("sen(", "sin(");
        funcionProcesada = funcionProcesada.Replace("ln(", "log(");

        // Agregar multiplicación implícita donde sea necesario
        funcionProcesada = AgregarMultiplicacionImplicita(funcionProcesada);

        return funcionProcesada;
    }

    private string AgregarMultiplicacionImplicita(string funcion)
    {
        string resultado = "";
        for (int i = 0; i < funcion.Length; i++)
        {
            resultado += funcion[i];

            // Si el carácter actual es un dígito y el siguiente es 'x' o '('
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
                MessageBox.Show("Por favor ingrese una función.");
                return;
            }

            double a = double.Parse(txtA.Text);
            double b = double.Parse(txtB.Text);
            double tol = double.Parse(txtTolerancia.Text);
            int maxIter = int.Parse(txtMaxIteraciones.Text);

            // Verificar que el intervalo sea válido
            if (a >= b)
            {
                MessageBox.Show("El valor de 'a' debe ser menor que 'b'.");
                return;
            }

            double fa = EvaluarFuncion(funcion, a);
            double fb = EvaluarFuncion(funcion, b);

            if (fa * fb >= 0)
            {
                MessageBox.Show($"La función debe cambiar de signo entre a y b.\nf({a}) = {fa:F6}\nf({b}) = {fb:F6}");
                return;
            }

            string resultado = "";
            double c = a;
            double fc;
            int iter = 0;
            double error;

            // Encabezado
            resultado += "MÉTODO DE LA POSICIÓN FALSA (REGLA FALSI)\n";
            resultado += new string('=', 50) + "\n";
            resultado += $"Función: {funcion}\n";
            resultado += $"Intervalo inicial: [{a}, {b}]\n";
            resultado += $"f({a}) = {fa:F6}, f({b}) = {fb:F6}\n";
            resultado += $"Tolerancia: {tol}\n";
            resultado += $"Máximo de iteraciones: {maxIter}\n\n";

            do
            {
                fa = EvaluarFuncion(funcion, a);
                fb = EvaluarFuncion(funcion, b);

                // Fórmula del método de la posición falsa
                c = (a * fb - b * fa) / (fb - fa);
                fc = EvaluarFuncion(funcion, c);
                error = Math.Abs(fc);

                resultado += $"Iteración {iter + 1}:\n";
                resultado += $"  c = ({a:F6} × {fb:F6} - {b:F6} × {fa:F6}) / ({fb:F6} - {fa:F6})\n";
                resultado += $"  c = {c:F6}\n";
                resultado += $"  f(c) = f({c:F6}) = {fc:F6}\n";
                resultado += $"  Error = |f(c)| = {error:F6}\n";

                // Determinar el nuevo intervalo
                if (fa * fc < 0)
                {
                    resultado += $"  f(a)×f(c) < 0, nuevo intervalo: [{a:F6}, {c:F6}]\n";
                    b = c;
                }
                else
                {
                    resultado += $"  f(a)×f(c) > 0, nuevo intervalo: [{c:F6}, {b:F6}]\n";
                    a = c;
                }

                resultado += "\n";
                iter++;

                if (iter >= maxIter)
                {
                    resultado += $"Se alcanzó el máximo número de iteraciones ({maxIter}).\n";
                    break;
                }

            } while (error > tol);

            if (error <= tol)
            {
                resultado += "CONVERGENCIA ALCANZADA\n";
                resultado += new string('-', 30) + "\n";
                resultado += $"Raíz aproximada: {c:F8}\n";
                resultado += $"Error final: {error:F8}\n";
                resultado += $"Número de iteraciones: {iter}\n";
                resultado += $"Verificación: f({c:F8}) = {fc:F8}";
            }

            txtResultado.Text = resultado;
        }
        catch (Exception ex)
        {
            string mensaje = "Error: " + ex.Message + "\n\n";
            mensaje += "Ejemplos de funciones válidas:\n";
            mensaje += "• Polinomios: x^2 - 4, x^3 - 2*x - 1\n";
            mensaje += "• Con pow(): pow(x,2) - 4, pow(x,3) - 2*x - 1\n";
            mensaje += "• Trigonométricas: sin(x) - 0.5, cos(x) - x\n";
            mensaje += "• Exponenciales: exp(x) - 2, log(x) - 1\n";
            mensaje += "• Mixtas: x^2 + sin(x) - 1\n\n";
            mensaje += "Nota: Use 'x' como variable independiente";

            MessageBox.Show(mensaje);
        }
    }
}
