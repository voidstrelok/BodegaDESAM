using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace BodegaDESAM.Contract.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class RutChilenoAttribute : ValidationAttribute
{
    private static readonly Regex FormatoRut = new(
        @"^\d{1,2}\.\d{3}\.\d{3}-[\dK]$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public RutChilenoAttribute()
        : base("El RUT debe ser válido y tener el formato 12.345.678-9.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string rut || !FormatoRut.IsMatch(rut))
            return false;

        var partes = rut.Split('-');
        var cuerpo = partes[0].Replace(".", string.Empty);

        if (cuerpo.All(digito => digito == '0'))
            return false;

        var suma = 0;
        var multiplicador = 2;

        for (var indice = cuerpo.Length - 1; indice >= 0; indice--)
        {
            suma += (cuerpo[indice] - '0') * multiplicador;
            multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
        }

        var resultado = 11 - (suma % 11);
        var digitoVerificador = resultado switch
        {
            11 => '0',
            10 => 'K',
            _ => (char)('0' + resultado)
        };

        return partes[1][0] == digitoVerificador;
    }
}
