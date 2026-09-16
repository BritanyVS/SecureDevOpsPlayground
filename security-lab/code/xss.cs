using System.Net;

namespace SnykLabCode.Scenarios;

// ⚠️ LABORATORIO: CROSS-SITE SCRIPTING (XSS)
// Código INTENCIONALMENTE vulnerable para que Snyk Code lo detecte.

public class XssLab
{
    // VULNERABLE: el input del usuario se inserta en HTML sin escapar.
    // request (/search?q=<script>alert(1)</script>) → respuesta HTML contaminada.
    public string RenderUserInputHtml(string input)
    {
        // FALLO: concatenación directa → Reflected XSS.
        return $"<html><body><p>Resultados para: {input}</p></body></html>";
    }

    // ✅ CORREGIDO: se codifica el HTML antes de insertarlo.
    public string RenderUserInputHtmlSecure(string input)
    {
        var safe = WebUtility.HtmlEncode(input);
        return $"<html><body><p>Resultados para: {safe}</p></body></html>";
    }

    // VULNERABLE (stored/React): si el SPA usa dangerouslySetInnerHTML con input del usuario.
    public object RenderReactCard(string userText)
    {
        // FALLO: si esto llegara a dangerouslySetInnerHTML, es XSS almacenado.
        return new { html = $"<div class='card'>{userText}</div>" };
    }

    // ✅ CORREGIDO: en React, renderizar como texto (nunca HTML de usuario).
    public string ReactSafeRender(string userText)
    {
        return userText; // El framework lo escapa por defecto si se pasa como {userText}.
    }
}