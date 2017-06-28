using Amazon.S3.Model;
using AmazonSyncADUsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ExtranetChina.Controllers
{
    public class ViewPDFAmazonController : Controller
    {
        //
        // GET: /ViewPDFAmazon/
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDocument(int P_ARBRFART_ID, int P_CDIDIOMA, string P_IMPRES_DETALHE)
        {
            try
            {
                if (P_ARBRFART_ID == null || P_CDIDIOMA == null || string.IsNullOrEmpty(P_IMPRES_DETALHE))
                    return null;

                var manager = new RequestManager();
                string hidden_run_parameters = "hidden_run_parameters=server=rep_bnsvher280_rep10g&report=%2Fhome%2Freport%2Foracle10g%2Freports%2Fhome%2Fcoml%2Fsist%2Far0148.rdf&destype=cache&desformat=pdf&userid=COML%2FCOML%40CORP1_qa&P_ARBRFART_ID={0}&PARAMFORM=YES&P_INDPERCCOMPOS=S&P_IMPRES_INSUMO=S&P_IMPRES_IMAGEM=S&P_IMPRES_MEDIDA=S&P_CDIDIOMA={1}&P_IMPRES_DETALHE={2}";
                string content = string.Format(hidden_run_parameters, P_ARBRFART_ID, P_CDIDIOMA, P_IMPRES_DETALHE);
                string uri = "http://bnsvher280.hering.local:7777/reports/rwservlet?";

                var dados = manager.GetResponseContent(manager.SendPOSTRequest(uri, content, null, null, true));

                ViewBag.AmazonUrl = AmazonS3Helper.CreateFileShare(P_ARBRFART_ID.ToString(), dados);
                                
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.ToString();
                return View();
            }
        }
	}
}