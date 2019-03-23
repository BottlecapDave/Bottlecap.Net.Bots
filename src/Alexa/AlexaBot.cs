using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Bottlecap.Authentication;
using Bottlecap.Components.Bots.Alexa.Data;
using Bottlecap.Components.Bots.Alexa.Web;
using Bottlecap.Components.Bots.Web;
using Bottlecap.Json;
using Bottlecap.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bottlecap.Components.Bots.Alexa
{
    public class AlexaBot : IBot
    {
        public string GeolocationApiKey { get { return "AIzaSyBcuF-nlaP4IbOuaUDjf2qeH7pS8HGjMlI"; } }

        public Token AuthToken { get; private set; }

        private IBotQuery _query;

        public IBotQuery Query
        {
            get
            {
                if (_query == null)
                {
                    var intent = _request.Request as IntentRequest;
                    if (intent != null)
                    {
                        _query = new AlexaQuery(intent);
                    }
                }

                return _query;
            }
        }

        public string Locale
        {
            get
            {
                return _request.Request.Locale;
            }
        }

        private Pair<decimal, decimal>? _location;

        private Bots.Data.Address _address;

        private SkillRequest _request;

        private ILambdaContext _context;

        private bool _addressOnlySupportsPostalCode;

        public AlexaBot(SkillRequest request, ILambdaContext context, bool addressOnlySupportsPostalCode = true)
        {
            _request = request;
            _context = context;
            _addressOnlySupportsPostalCode = addressOnlySupportsPostalCode;

            Initialise();
        }

        public SkillResponse ToSkillResponse(IBotResponse response)
        {
            IOutputSpeech outputSpeech;
            if (response.IsSSML)
            {
                outputSpeech = new SsmlOutputSpeech()
                {
                    Ssml = String.Format("<speak>{0}</speak>",
                                         response.Speak ?? String.Empty)
                };
            }
            else
            {
                outputSpeech = new PlainTextOutputSpeech { Text = response.Speak ?? String.Empty };
            }

            SkillResponse nativeResponse = null;
            if (response.PermissionRequests.HasFlag(PermissionType.Address))
            {
                var permissions = new List<string>();
                permissions.Add(_addressOnlySupportsPostalCode ? "read::alexa:device:all:address:country_and_postal_code" : "read::alexa:device:all:address");

                nativeResponse = ResponseBuilder.TellWithAskForPermissionConsentCard(response.Speak, permissions, _request.Session);
            }
            else if (response.Content != null)
            {
                var card = response.Content as ICard;
                if (card != null)
                {
                    nativeResponse = ResponseBuilder.TellWithCard(outputSpeech, response.Title ?? String.Empty, JsonFactory.Current.ToJson(card), _request.Session);
                }
                else
                {
                    nativeResponse = ResponseBuilder.TellWithCard(outputSpeech, response.Title ?? String.Empty, response.Content.ToString(), _request.Session);
                }
            }
            
            if (nativeResponse == null)
            {
                nativeResponse = ResponseBuilder.Tell(outputSpeech, _request.Session);
            }

            nativeResponse.Response.ShouldEndSession = response.ExpectedUserResponse == UserResponse.None ? true : false;

            return nativeResponse;
        }

        public void Log(string message)
        {
            _context.Logger.Log(message);
        }

        public void Log(string message, params object[] parameters)
        {
            _context.Logger.Log(String.Format(message, parameters));
        }

        private void Initialise()
        {
            if (String.IsNullOrEmpty(_request.Session?.User?.AccessToken) == false)
            {
                AuthToken = new Token(_request.Session?.User?.AccessToken, "Bearer");
            }

            AuthenticationContext.Initialise(accessToken: AuthToken);
        }

        public async Task<Bots.Data.Address> GetAddressAsync()
        {
            if (_address == null)
            {
                var consentToken = _request.Context?.System?.ApiAccessToken;
                var deviceId = _request.Context?.System?.Device?.DeviceID;
                var baseUri = _request.Context?.System?.ApiEndpoint;

                if (String.IsNullOrEmpty(deviceId) == false &&
                    String.IsNullOrEmpty(consentToken) == false &&
                    String.IsNullOrEmpty(baseUri) == false)
                {
                    var addressRequest = new GetAddressRequest(consentToken, baseUri, deviceId, _addressOnlySupportsPostalCode);
                    if (await WebRequestManager.MakeRequestAsync(addressRequest) &&
                        addressRequest.Response != null)
                    {
                        Log("Address retrieved");

                        _address = new Bots.Data.Address()
                        {
                            AddressLine1 = addressRequest.Response.addressLine1,
                            AddressLine2 = addressRequest.Response.addressLine2,
                            AddressLine3 = addressRequest.Response.addressLine3,
                            City = addressRequest.Response.city,
                            Postcode = addressRequest.Response.postalCode,
                            CountryCode = addressRequest.Response.countryCode
                        };
                    }
                    else
                    {
                        Log("Failed to retrieve address. {0}", addressRequest.RawResponse);
                    }
                }
                else
                {
                    Log("Device and/or consent token not available");
                }
            }

            return _address;
        }

        public async Task<Pair<decimal, decimal>?> GetLocationAsync()
        {
            if (_location == null)
            {
                var consentToken = _request.Context?.System?.ApiAccessToken;
                var deviceId = _request.Context?.System?.Device?.DeviceID;
                var baseUri = _request.Context?.System?.ApiEndpoint;

                if (String.IsNullOrEmpty(deviceId) == false &&
                    String.IsNullOrEmpty(consentToken) == false &&
                    String.IsNullOrEmpty(baseUri) == false)
                {
                    var addressRequest = new GetAddressRequest(consentToken, baseUri, deviceId, _addressOnlySupportsPostalCode);
                    if (await WebRequestManager.MakeRequestAsync(addressRequest))
                    {
                        if (String.IsNullOrEmpty(addressRequest.Response.postalCode) == false)
                        {
                            var geoLocationRequest = new GeolocationRequest(GeolocationApiKey, addressRequest.Response.postalCode);
                            if (await WebRequestManager.MakeRequestAsync(geoLocationRequest))
                            {
                                var result = geoLocationRequest.Response.results.FirstOrDefault();
                                if (result.geometry?.location != null)
                                {
                                    _location = new Pair<decimal, decimal>(result.geometry.location.lat, result.geometry.location.lng);
                                }
                            }
                        }
                    }
                }
            }

            return _location;
        }

        public async Task SendProgressResponseAsync(IBotResponse response)
        {
            var directive = new Directive()
            {
                header = new DirectiveHeader()
                {
                    requestId = _request.Request.RequestId
                },
                directive = new DirectiveContent()
                {
                    speech = response.Speak
                }
            };

            if (String.IsNullOrEmpty(_request.Context.System.ApiEndpoint) == false &&
                String.IsNullOrEmpty(_request.Context.System.ApiAccessToken) == false)
            {
                var request = new DirectiveRequest(_request.Context.System.ApiAccessToken, _request.Context.System.ApiEndpoint, directive);

                if (await WebRequestManager.MakeRequestAsync(request))
                {
                    Log("Progress Response succesful");
                }
                else
                {
                    Log("Progress Response unsuccessful. Response: {0}", request.RawResponse);
                }
            }
            else
            {
                Log("Progress Response unsuccessful. API endpoint or access token was not avsilable.");
            }
        }

        public Task AddContextAsync(string key, object data)
        {
            if (_request.Session.Attributes == null)
            {
                _request.Session.Attributes = new Dictionary<string, object>();
            }

            if (_request.Session.Attributes.ContainsKey(key))
            {
                _request.Session.Attributes.Remove(key);
            }

            _request.Session.Attributes.Add(key, data);
            return Task.FromResult(true);
        }

        public Task<T> GetContextAsync<T>(string key)
            where T : class
        {
            Log("GetContextAsync - {0}", key);

            object value = null;
            if (_request.Session?.Attributes?.TryGetValue(key, out value) == true)
            {
                var specificValue = value as T;
                if (specificValue != null)
                {
                    Log("GetContextAsync Successful");
                }
                else if (value != null)
                {
                    specificValue = JsonFactory.Current.ReadJson<T>(value.ToString());
                    if (specificValue != null)
                    {
                        Log("GetContextAsync Successful");
                    }
                }
                
                if (specificValue == null)
                {
                    Log("GetContextAsync unsuccessful {0}", value?.ToString());
                }

                return Task.FromResult<T>(specificValue);
            }

            return Task.FromResult<T>(null);
        }
    }
}