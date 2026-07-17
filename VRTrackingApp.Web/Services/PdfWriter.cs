using System.Text;

namespace VRTrackingApp.Web.Services;

/// <summary>Minimal single-page PDF writer (no external deps). Text lines only.</summary>
public static class PdfWriter
{
    public static byte[] Write(string title, IEnumerable<string> lines)
    {
        var text = new StringBuilder();
        text.Append("BT\n/F1 12 Tf\n24 812 Td\n");
        text.Append($"({Escape(title)}) Tj\n");
        text.Append("0 -22 Td\n/F1 9 Tf\n");
        foreach (var line in lines)
            text.Append($"({Escape(line)}) Tj\n0 -13 Td\n");
        text.Append("ET");

        var contentStr = text.ToString();
        var contentObj = 1;
        var pageObj = 2;
        var fontObj = 3;
        var pagesObj = 4;
        var catalogObj = 5;

        var objects = new Dictionary<int, string>
        {
            [contentObj] = $"<< /Length {Encoding.UTF8.GetByteCount(contentStr)} >>\nstream\n{contentStr}\nendstream",
            [pageObj] = $"<< /Type /Page /Parent {pagesObj} 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 {fontObj} 0 R >> >> /Contents {contentObj} 0 R >>",
            [fontObj] = "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
            [pagesObj] = $"<< /Type /Pages /Kids [{pageObj} 0 R] /Count 1 >>",
            [catalogObj] = $"<< /Type /Catalog /Pages {pagesObj} 0 R >>"
        };

        var body = new StringBuilder();
        var offsets = new int[objects.Count + 1];
        for (int i = 1; i <= objects.Count; i++)
        {
            offsets[i] = body.Length;
            body.Append($"{i} 0 obj\n{objects[i]}\nendobj\n");
        }
        int xrefPos = body.Length;
        var xref = new StringBuilder();
        xref.Append($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");
        for (int i = 1; i <= objects.Count; i++)
            xref.Append($"{offsets[i]:010} 00000 n \n");
        var trailer = $"trailer\n<< /Size {objects.Count + 1} /Root {catalogObj} 0 R >>\nstartxref\n{xrefPos}\n%%EOF";

        var sb = new StringBuilder();
        sb.Append("%PDF-1.4\n").Append(body).Append(xref).Append(trailer);
        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Escape(string s) => (s ?? "").Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
}
