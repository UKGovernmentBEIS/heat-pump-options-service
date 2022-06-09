using Microsoft.AspNetCore.Http;
using OCC.HSM.Analysis;
using OCC.HSM.Model.Entities;
using OCC.HSM.Model.Interfaces;
using System;
using System.Linq;
using System.Reflection;

namespace OCC.HSM.AnalysisService
{
    /// <summary>
    /// An implementation of the <see cref="IAnalysisService"/> which provides access to the analysis engine.
    /// </summary>
    public sealed class AnalysisService : IAnalysisService
    {
        /// <summary>
        /// For logging to a local logfile.
        /// </summary>
        private readonly ILogger _logger;

        private readonly IEohMemoryCache _eohMemoryCache;

        /// <summary>
        /// The questions being asked are contained within the configuration.
        /// </summary>
        private IApplicationConfiguration _configuration { get; }

        private readonly IHttpContextAccessor _httpContextAccessor;

        public AnalysisService(
            ILogger logger,
            IEohMemoryCache eohMemoryCache,
            IHttpContextAccessor httpContextAccessor,
            IApplicationConfiguration configuration
            )
        {
            _logger = logger;
            _eohMemoryCache = eohMemoryCache;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Send the answers to the analysis engine and collect the results.
        /// </summary>
        /// <param name="answers">The answers provided by the user.</param>
        /// <returns></returns>       
        public Eoh GetResult(HsmKey answers)
        {
            if (answers == null)
            {
                return new Eoh();
            }

            var eoh = _eohMemoryCache.Results[answers];
            return eoh;
        }

        /// <summary>
        /// Gets the user choices from the user Session.
        /// </summary>
        /// <returns>Returns a <see cref="HsmKey"> object containing all the user answer choices.</returns>
        public HsmKey GetUserChoices()
        {
            HsmKey hsmKey = new HsmKey();

            try
            {
                _configuration?.Questions?.ToList().ForEach(x =>
                {
                    var prop = hsmKey.GetType().GetProperty(x.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (prop != null)
                    {
                        var choiceFullKey = _httpContextAccessor.HttpContext.Session.GetString(x.Key);
                        var answerChoice = x.AnswerChoices.FirstOrDefault(x => x.Key == choiceFullKey);
                        if (choiceFullKey != null && choiceFullKey.Contains($"{x.Key}-", StringComparison.OrdinalIgnoreCase))
                        {
                            var choice = choiceFullKey.Replace($"{x.Key}-", string.Empty, StringComparison.Ordinal);

                            //If the question is "How many bedrooms does your home have?" then convert into floor area and set the property HouseSize
                            //1 bedroom = < 50m2
                            //2 bedrooms = 70 - 90m2
                            //3 bedrooms = 90 - 110m2
                            //4 bedrooms = 110 - 200m2
                            //5 + bedrooms = 110 - 200m2
                            if (x.Key.Equals("housesizeoption2"))
                            {
                                switch (choice)
                                {
                                    case "one":
                                        hsmKey.HouseSizeOption1 = 1;
                                        break;
                                    case "two":
                                        hsmKey.HouseSizeOption1 = 3;
                                        break;
                                    case "three":
                                        hsmKey.HouseSizeOption1 = 4;
                                        break;
                                    case "four":
                                        hsmKey.HouseSizeOption1 = 5;
                                        break;
                                    case "five-plus":
                                        hsmKey.HouseSizeOption1 = 5;
                                        break;
                                }
                            }
                            //If the question is Wall construction and the answer Don't know then based on house age set WallType property to 1,3 or 4
                            //Include "Don't know" which maps to wall type based on age?
                            //pre 1929 = solid wall 
                            //1930 - 1995 = cavity wall - uninsulated
                            //1996 onwards = cavity wall insulated
                            else if (x.Key.Equals("walltype"))
                            {
                                if (choice.Equals("dont-know") && hsmKey?.HouseAge != null)
                                {
                                    switch (hsmKey.HouseAge)
                                    {
                                        case 1:
                                        case 2:
                                            hsmKey.WallType = 1; //Solid uninsulated
                                            break;
                                        case 3:
                                        case 4:
                                        case 5:
                                        case 6:
                                        case 7:
                                        case 8:
                                            hsmKey.WallType = 3; //Cavity - unfilled
                                            break;
                                        case 9:
                                        case 10:
                                        case 11:
                                        case 12:
                                            hsmKey.WallType = 4; //Cavity - filled
                                            break;
                                    }
                                }
                                else
                                {
                                    hsmKey.WallType = (ushort?)answerChoice?.DbEncoding;
                                }
                            }
                            //Get the database encoding from the questions.xml file                       
                            else
                            {
                                ushort? choiceVal = (ushort?)answerChoice?.DbEncoding;
                                if (choiceVal > 0)
                                {
                                    prop.SetValue(hsmKey, choiceVal);
                                }
                            }
                        }
                    }
                });

                return hsmKey;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error occurred in AnalysisService.GetUserChoices(). Exception: {ex}");
            }

            return null;
        }
    }
}
