﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

using Newtonsoft.Json;
using log4net;
using captcha;
using Digest;
using Lingvistics.Client;
using OpinionMining;
using TextMining.Core;
using TonalityMarking;

namespace TonalityMarkingAndDigestInProc.web.demo
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RESTProcessHandler : IHttpHandler
    {
        /// <summary>
        /// 
        /// </summary>
        internal sealed class Result
        {
            public Result( Exception ex ) 
            {
                ErrorMessage = ex.ToString();
            }
            public Result( string html, TimeSpan elapsed )
            {
                Html    = html;
                Elapsed = elapsed;
            }

            [JsonProperty(PropertyName="error")]
            public string ErrorMessage
            {
                get;
                private set;
            }

            [JsonProperty(PropertyName="html")]
            public string Html
            {
                get;
                private set;
            }

            [JsonProperty(PropertyName="elapsed")]
            public TimeSpan Elapsed
            {
                get;
                private set;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal enum ProcessTypeEnum
        {
            TonalityMarking,
            Digest,
        }
        /// <summary>
        /// 
        /// </summary>
        private enum OutputTypeEnum
        {
            Xml,
            Xml_Custom,
            Html_FinalTonality,
            Html_FinalTonalityDividedSentence,
            Html_ToplevelTonality,
            Html_ToplevelTonalityDividedSentence,
            Html_BackcolorAllTonality,
        }
        /// <summary>
        /// 
        /// </summary>
        private struct LocalParams
        {
            public LocalParams( HttpContext context ) : this()
            {
                Context = context;
            }

            public HttpContext Context { get; private set; }
            public string Text { get; set; }
            public ProcessTypeEnum ProcessType { get; set; }
            public OutputTypeEnum OutputType { get; set; }
            public string InquiryText { get; set; }
            public ObjectAllocateMethod? ObjectAllocateMethod { get; set; }

            public bool TextIsDummy
            {
                get { return (Text == "_dummy_"); }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private static class ConcurrentFactoryHelper
        {
            private static object _SyncLock = new object();

            private static ConcurrentFactory _ConcurrentFactory;

            private static ConcurrentFactory GetConcurrentFactory()
            {
                var f = _ConcurrentFactory;
                if ( f == null )
                {
                    lock ( _SyncLock )
                    {
                        f = _ConcurrentFactory;
                        if ( f == null )
                        {
                            {
                                var ip = new ConcurrentFactory.InputParams()
                                {
                                    //InstanceCount            = Config.CONCURRENT_FACTORY_INSTANCE_COUNT,
                                    MaxEntityLength          = Config.MAX_ENTITY_LENGTH,
                                    UseCoreferenceResolution = Config.USE_COREFERENCE_RESOLUTION,
                                    UseGeoNamesDictionary    = Config.USE_GEONAMES_DICTIONARY,
                                };           
                                f = new ConcurrentFactory( ip );                                
                            }
                            {
                                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
                                GC.WaitForPendingFinalizers();
                                GC.Collect( GC.MaxGeneration, GCCollectionMode.Forced );
                            }

                            _ConcurrentFactory = f;
                        }
                    }
                }
                return (f);
            }

            public static LingvisticsResult ProcessText( LingvisticsTextInput input )
            {
                return (GetConcurrentFactory().ProcessText( input ));
            }
        }

        static RESTProcessHandler()
        {
            //IMPORTANT for TM-OM resources
            Environment.CurrentDirectory = HttpContext.Current.Server.MapPath( "~/" );

            try
            {
                log4net.Config.XmlConfigurator.Configure();
            }
            catch ( Exception ex )
            {                
                Debug.WriteLine( ex );
            }
        }

        public bool IsReusable
        {
            get { return (true); }
        }

        public void ProcessRequest( HttpContext context )
        {
            try
            {
                #region [.anti-bot.]
                var antiBot = context.ToAntiBot();
                if ( antiBot.IsNeedRedirectOnCaptchaIfRequestNotValid() )
                {
                    antiBot.SendGotoOnCaptchaJsonResponse();
                    return;
                }
                #endregion

                var lp = new LocalParams( context )
                {
                    Text                 = context.GetRequestStringParam( "text", Config.MAX_INPUTTEXT_LENGTH ),
                    ProcessType          = context.Request[ "processType" ].TryConvert2Enum< ProcessTypeEnum >().GetValueOrDefault( ProcessTypeEnum.TonalityMarking ),
                    OutputType           = (context.Request[ "splitBySentences" ].TryToBool( false ) ? OutputTypeEnum.Html_FinalTonalityDividedSentence : OutputTypeEnum.Html_FinalTonality),
                    InquiryText          = context.GetRequestStringParam( "inquiryText", Config.MAX_INPUTTEXT_LENGTH ),
                    ObjectAllocateMethod = context.Request[ "objectAllocateMethod" ].TryToEnum< ObjectAllocateMethod >(),
                };

                #region [.anti-bot.]
                antiBot.MarkRequestEx( lp.Text );
                #endregion

                var sw = Stopwatch.StartNew();
                var html = GetResultHtml( lp );
                sw.Stop();

                context.Response.SendJson( html, sw.Elapsed );
            }
            catch ( Exception ex )
            {
                context.Response.SendJson( ex );
            }
        }

        private static string GetResultHtml( LocalParams lp )
        {
            var lingvisticsInput = new LingvisticsTextInput()
			{
				Text                 = lp.Text,
				AfterSpellChecking   = false,
				BaseDate             = DateTime.Now,
				Mode                 = SelectEntitiesMode.Full,
				GenerateAllSubthemes = false, 
			};

            try
            {
                var html = default(string);

                #region [.result.]
                switch ( lp.ProcessType )
                {
                    case ProcessTypeEnum.Digest:
                    #region [.code.]
                    {
                        lingvisticsInput.Options = LingvisticsResultOptions.OpinionMiningWithTonality;

                        var lingvisticResult = ConcurrentFactoryHelper.ProcessText( lingvisticsInput );

                        html = ConvertToHtml( lp.Context, lingvisticResult.OpinionMiningWithTonalityResult );
                    }
                    break;
                    #endregion

                    case ProcessTypeEnum.TonalityMarking:
                    #region [.code.]
                    {
                        lingvisticsInput.TonalityMarkingInput = new TonalityMarkingInputParams4InProcess();
                        if ( !lp.InquiryText.IsNullOrWhiteSpace() )
                        {
                            lingvisticsInput.TonalityMarkingInput.InquiriesSynonyms = lp.InquiryText.ToTextList();
                        }
                        lingvisticsInput.ObjectAllocateMethod = lp.ObjectAllocateMethod.GetValueOrDefault( ObjectAllocateMethod.FirstVerbEntityWithRoleObj );
                        lingvisticsInput.Options = LingvisticsResultOptions.Tonality;                    

                        var lingvisticResult = ConcurrentFactoryHelper.ProcessText( lingvisticsInput );

                        html = ConvertToHtml( lp.Context, lingvisticResult.TonalityResult, lp.OutputType );
                    }
                    break;
                    #endregion

                    default:
                        throw (new ArgumentException( lp.ProcessType.ToString() ));
                }
                #endregion


                if ( !lp.TextIsDummy )
                {
                    LogManager.GetLogger( string.Empty )?.Info( $"'{lp.ProcessType}', TEXT: '{lp.Text}'" );
                }

                return (html);
            }
            catch ( Exception ex )
            {
                LogManager.GetLogger( string.Empty )?.Error( $"'{lp.ProcessType}', TEXT: '{lp.Text}'", ex );
                throw;
            }
        }

        private static string ConvertToHtml( HttpContext context, TonalityMarkingOutputResult result, OutputTypeEnum outputType )
        {
            var xdoc = new XmlDocument(); 
            xdoc.LoadXml( result.OutputXml );

            var xslt = new XslCompiledTransform( false );

            var xsltFilename = default(string);
            switch ( outputType )
            {
                case OutputTypeEnum.Xml_Custom:
                    xsltFilename = "Xml.xslt";
                    break;
                case OutputTypeEnum.Html_ToplevelTonality:
                    xsltFilename = "ToplevelTonality.xslt"; 
                    break;
                case OutputTypeEnum.Html_ToplevelTonalityDividedSentence:
                    xsltFilename = "ToplevelTonalityDividedSentence.xslt";
                    break;
                case OutputTypeEnum.Html_FinalTonality:
                    xsltFilename = "FinalTonality.xslt"; 
                    break;
                case OutputTypeEnum.Html_FinalTonalityDividedSentence:
                    xsltFilename = "FinalTonalityDividedSentence.xslt";
                    break;
                case OutputTypeEnum.Html_BackcolorAllTonality:
                    xsltFilename = "BackcolorAllTonality.xslt"; 
                    break;
                default:
                    throw (new ArgumentException(outputType.ToString()));
            }

            xslt.Load( context.Server.MapPath( "~/App_Data/" + xsltFilename ) );

            var sb = new StringBuilder();
            using ( var sw = new StringWriter( sb ) )
            {
                xslt.Transform( xdoc.CreateNavigator(), null, sw );
            }
            return (sb.ToString());
        }

        private static string ConvertToHtml( HttpContext context, DigestOutputResult result )
        {
            const string ANYTHING_ISNT_PRESENT = "<span style='color: maroon; font-size: x-small;'>[Ничего нет.]</span>";
            const string TABLE_START           = "<table border='1' style='font-size: x-small;'><tr><th>#</th><th>SUBJECT</th><th>OBJECT</th><th>SENTENCE</th></tr>";
            const string TABLE_END             = "</table>";
            const string TABLE_ROW_FORMAT      = "<tr valign='top'><td>{0}</td><td>&nbsp;{1}</td><td>&nbsp;{2}</td><td style='padding: 3px;'>{3}</td></tr>";

            if ( !result.Tuples.Any() )
            {
                return (ANYTHING_ISNT_PRESENT);
            }                

            const string XSLT_FILENAME = "FinalTonality.Digest.test.xslt";

            var xslt = new XslCompiledTransform( false );
            xslt.Load( context.Server.MapPath( "~/App_Data/" + XSLT_FILENAME ) );

            var tmp = new StringBuilder();
            var sb = new StringBuilder( TABLE_START );
            var number = 0;
            foreach ( var tuple in result.Tuples )
            {
                sb.AppendFormat
                (
                TABLE_ROW_FORMAT,
                (++number),
                tuple.Subject.ToHtml(),
                tuple.Object .ToHtml(),
                tuple.GetSentence().Transform( xslt, tmp )
                );
            }
            sb.Append( TABLE_END );

            return (sb.ToString());
        }
    }

    /// <summary>
    /// 
    /// </summary>
    internal static partial class Extensions
    {
        public static string GetRequestStringParam( this HttpContext context, string paramName, int maxLength )
        {
            var value = context.Request[ paramName ];
            if ( (value != null) && (maxLength < value.Length) && (0 < maxLength) )
            {
                return (value.Substring( 0, maxLength ));
            }
            return (value);
        }

        public static List< string > ToTextList( this string text )
        {
            return (text.Split( new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries ).ToList());
        }


        public static string ToHtml( this SubjectEssence subject )
        {
            return (subject.IsAuthor ? "<span style='color: silver;'>{0}</span>" : "{0}").FormatEx( subject.ToString() );
        }
        public static string ToHtml( this ObjectEssence @object )
        {
            return ((@object != null) ? (@object + ((@object.IsSubjectIndeed) ? "<span style='color: silver;'>&nbsp;(субъект-как-объект)</span>" : string.Empty)) : "-");
        }

        public static string Transform( this XElement xe, XslCompiledTransform xslt, StringBuilder sb )
        {
            if ( sb == null ) sb = new StringBuilder();
            sb.Clear();

            using ( var sw = new StringWriter( sb ) )
            {
                xslt.Transform( xe.CreateReader(), null, sw );
            }
            return (sb.ToString());
        }


        public static void SendJson( this HttpResponse response, string html, TimeSpan elapsed )
        {
            response.SendJson( new RESTProcessHandler.Result( html, elapsed ) );
        }
        public static void SendJson( this HttpResponse response, Exception ex )
        {
            response.SendJson( new RESTProcessHandler.Result( ex ) );
        }
        public static void SendJson( this HttpResponse response, RESTProcessHandler.Result result )
        {
            response.ContentType = "application/json"; //"text/html"
            //---response.Headers.Add( "Access-Control-Allow-Origin", "*" );

            var json = JsonConvert.SerializeObject( result );
            response.Write( json );
        }
    }
}