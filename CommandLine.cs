using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;


namespace multi_start
{
    /// <summary>
    /// Classe descrevendo os parametros passados por linha de comando
    /// </summary>
    [Serializable]
    public class Parametros
    {
        private const string NULL_STR = "NULL";

        public Parametros()
        {
            CarregaParametrosDefault();
        }

        /// <summary>
        /// Carrega os parametros default
        /// </summary>
        private void CarregaParametrosDefault()
        {
            Type thisType = typeof (Parametros);
            foreach (PropertyInfo pInfo in thisType.GetProperties())
            {
                CommandParameterAttribute cmdParam = (CommandParameterAttribute) pInfo.GetCustomAttributes(typeof(CommandParameterAttribute), false).FirstOrDefault();
                if (cmdParam != null)
                    SalvaValorPropriedade(pInfo, cmdParam.DefaultValue);
            }

        }

        /// <summary>
        /// Salva o valor de uma propriedade
        /// </summary>
        /// <param name="pInfo">Parameter info a ser salvo</param>
        /// <param name="valorPropriedade">Valor da propriedade a ser salva</param>
        private void SalvaValorPropriedade(PropertyInfo pInfo, string valorPropriedade)
        {

            Type propertyType = pInfo.PropertyType;
            object valor = null;

            if (valorPropriedade == NULL_STR)
            {
                if (propertyType.IsGenericType &&
                        propertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                    valor = null;
                else
                    throw new Exception(string.Format ("Valor da propriedade {0} não pode ser nullo", pInfo.Name));
            }
            else if (propertyType.IsEnum)
            {
                FieldInfo fInfo = propertyType.GetFields().Where(s => s.Name.ToLower() == valorPropriedade.ToLower()).FirstOrDefault();
                if (fInfo != null)
                    valor = Enum.Parse(pInfo.PropertyType, fInfo.Name);
            }
            else if (propertyType == typeof(string))
            {

                valor = TrataPropriedadeString (valorPropriedade.ToString());
            }


            else if ((propertyType == typeof(int)) || (propertyType == typeof(int?)))
                valor = int.Parse(valorPropriedade.ToString());

            else if ((propertyType == typeof(DateTime)) || (propertyType == typeof(DateTime?)))
                valor = DateTime.Parse(valorPropriedade);

            else if ((propertyType == typeof(bool)) || (propertyType == typeof(bool?)))
                valor = (valorPropriedade.Length == 0) ? false : bool.Parse(valorPropriedade);

            else if ((propertyType == typeof(double)) || (propertyType == typeof(double?)))
                valor = (valorPropriedade.Length == 0) ? 0.0 : Double.Parse(valorPropriedade);

            else if (propertyType == typeof(string[]))
                valor = (valorPropriedade.ToString() == string.Empty) ? new string[] { } : valorPropriedade.ToString().Split(',');
            else
                return;

            pInfo.SetValue(this, valor, new object[] { });
        }

        /// <summary>
        /// Trata o valor da propriedade string
        /// </summary>
        /// <param name="p">valor da propriedade</param>
        /// <returns>Valor da propriedade tratada</returns>
        private string TrataPropriedadeString(string propValue)
        {
            string path = System.IO.Path.GetDirectoryName (Assembly.GetExecutingAssembly().Location);
            return propValue.Replace("[EXEPATH]", path);
        }

        /// <summary>
        /// Carrega os parametros da linha de comando
        /// </summary>
        /// <param name="args">Arqgumentos passados via linha de comando</param>
        public void Load(string[] args)
        {
            Type tipo = typeof (Parametros);

            var parametros = 
                from parameter in tipo.GetProperties()
                let cmdParam = (CommandParameterAttribute)parameter.GetCustomAttributes (typeof (CommandParameterAttribute), false).FirstOrDefault()
                where cmdParam != null
                select new
                {
                    ParamAttr = cmdParam,
                    Parametro = parameter,
                    Modifier = cmdParam.Modifier
                };


            for (int i = 0; i < args.Length; i++)
            {
                string modifier = args[i];
                var param = parametros.Where(s => s.Modifier.ToLower() == modifier.ToLower()).FirstOrDefault();

                if (param == null)
                    GeraErro (string.Format ("Modificador Inválido '{0}", modifier));

                if (param.Parametro.PropertyType == typeof(bool))
                {
                    bool v;
                    if (!((i < args.Length-1) && (bool.TryParse (args[i+1], out v))))
                        v = true;
                    else
                        i++;

                    SalvaValorPropriedade(param.Parametro, v.ToString());
                }
                else
                {
                    if (++i < args.Length)
                        SalvaValorPropriedade(param.Parametro, args[i]);
                }
            }
        }

        /// <summary>
        /// Gera uma mensagem de help do projeto
        /// </summary>
        /// <param name="mensagemErro">Mensagem de erro</param>
        /// <returns>Erro com o help</returns>
        private string GeraErro(string mensagemErro)
        {
            StringBuilder sb = new StringBuilder();
            TextWriter writer = new StringWriter (sb);
            writer.WriteLine (@"
{0}

USO:
    multi-start.exe [-Modificador] [Valor Parametro]

onde os modificadores e valores válidos são:", mensagemErro);

            Type tipo = this.GetType();
            foreach (PropertyInfo property in tipo.GetProperties())
            {
                CommandParameterAttribute cmdParamAttr = (CommandParameterAttribute)property.GetCustomAttributes (typeof (CommandParameterAttribute), false).FirstOrDefault();
                if (cmdParamAttr == null)
                    continue;

                DescriptionAttribute descAttribute = (DescriptionAttribute)property.GetCustomAttributes (typeof (DescriptionAttribute), false).FirstOrDefault();
                string desc = (descAttribute == null)? "[Sem descrição]" : descAttribute.Description;

                writer.WriteLine ("  {0}{1}{2}", cmdParamAttr.Modifier,
                                new string(' ', Math.Max (0, 15 - cmdParamAttr.Modifier.Length)) ,
                                desc);
            }

            throw new Exception(sb.ToString());

        }


        /// <summary>
        /// Caminho da pasta casos teste
        /// </summary>
        [CommandParameter("-f") ]
        [Description(@"Command to be started.")]
        public string Command { get; set; }


        [CommandParameter ("-p", "")]
        [Description(@"Command Parameters.")]
        public string Parameters { get; set; }

        [CommandParameter("-c", "1")]
        [Description(@"Number of commands to be executed")]
        public int Count { get; set; }

        /// <summary>
        /// Arquivo para visualizacao do resultado
        /// </summary>
        [CommandParameter("-s", "-1")]
        [Description(@"Screen to display processes.")]
        public int Screen { get; set; }

    }



    /// <summary>
    /// Valor Default de uma enumeracao
    /// </summary>
    [global::System.AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class CommandParameterAttribute : Attribute
    {
        public CommandParameterAttribute(string modifier, string defaultValue)
        {
            this.Modifier = modifier;
            this.DefaultValue = defaultValue;
        }

        public CommandParameterAttribute(string modifier)
            : this(modifier, string.Empty)
        {
        }
        public string DefaultValue { get; private set; }
        public string Modifier { get; private set; }
    }

}
