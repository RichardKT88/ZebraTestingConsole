// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using HtmlAgilityPack;

// Console.WriteLine("Hello, World!");
string printerIpAddress = "10.92.0.167";
string zpl = "^XA^CFD^CVY^PON^FWN^LS0^LT0^LH15,17^FS^FO0,2^FO14,3^FH^FDHi^FS^XZ";

// post the data to the printer
var imageName = PostZplAndReturnImageName(zpl, printerIpAddress);

// Get the image from the printer
var image = LoadImageFromPrinter(imageName, printerIpAddress);

Console.WriteLine(image);

static string PostZplAndReturnImageName(string zpl, string printerIpAddress)
{
    string response = null;
    // Setup the post parameters.
    string parameters = "data=" + zpl;
    parameters = parameters + "&" + "dev=R";
    parameters = parameters + "&" + "oname=UNKNOWN";
    parameters = parameters + "&" + "otype=ZPL";
    parameters = parameters + "&" + "prev=Preview Label";
    parameters = parameters + "&" + "pw=";

    // Post to the printer
    response = HttpPost("http://" + printerIpAddress + "/zpl", parameters);

    // Parse the response to get the image name.  This image name is stored for one retrieval only.
    HtmlAgilityPack.HtmlDocument doc = new HtmlDocument();
    doc.LoadHtml(response);
    var imageNameXPath = "/html[1]/body[1]/div[1]/img[1]/@alt[1]";
    var imageAttributeValue = doc.DocumentNode.SelectSingleNode(imageNameXPath).GetAttributeValue("alt", "");
    // Take off the R: from the front and the .PNG from the back.
    var imageName = imageAttributeValue.Substring(2);
    imageName = imageName.Substring(0, imageName.Length - 4);

    // Return the image name.
    return imageName;
}

static string HttpPost(string URI, string Parameters)
{
    System.Net.WebRequest req = System.Net.WebRequest.Create(URI);
    req.Proxy = new System.Net.WebProxy();

    //Add these, as we're doing a POST
    req.ContentType = "application/x-www-form-urlencoded";
    req.Method = "POST";

    //We need to count how many bytes we're sending. 
    //Post'ed Faked Forms should be name=value&
    byte[] bytes = System.Text.Encoding.ASCII.GetBytes(Parameters);
    req.ContentLength = bytes.Length;

    System.IO.Stream os = req.GetRequestStream();
    os.Write(bytes, 0, bytes.Length); //Push it out there
    os.Close();

    System.Net.WebResponse resp = req.GetResponse();

    if (resp == null) return null;
    System.IO.StreamReader sr =
          new System.IO.StreamReader(resp.GetResponseStream());
    return sr.ReadToEnd().Trim();
}

static Image LoadImageFromPrinter(string imageName, string printerIpAddress)
{
    string url = "http://" + printerIpAddress + "/png?prev=Y&dev=R&oname=" + imageName + "&otype=PNG";

    var response = Http.Get(url);

    using (var ms = new MemoryStream(response))
    {
        Image image = Image.FromStream(ms);

        return image;
    }


}

public static class Http
{
    public static byte[] Get(string uri)
    {
        byte[] response = null;
        using (WebClient client = new WebClient())
        {
            response = client.DownloadData(uri);
        }
        return response;
    }
}