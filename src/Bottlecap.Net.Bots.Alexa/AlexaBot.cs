using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Bottlecap.Net.Bots.Alexa.Data;
using Bottlecap.Net.Bots.Alexa.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bottlecap.Net.Bots.Alexa
{
    public class AlexaBot : IBot
    {
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

        private Tuple<decimal, decimal> _location;

        private Bots.Data.Address _address;

        private readonly SkillRequest _request;
        private readonly ILambdaContext _context;
        private readonly bool _addressOnlySupportsPostalCode;
        private readonly string _geolocationApiKey;
        private readonly AddressService _addressService = new AddressService();

        public AlexaBot(SkillRequest request, 
                        ILambdaContext context,
                        bool addressOnlySupportsPostalCode = true,
                        string geolocationApiKey = null)
        {
            _request = request;
            _context = context;
            _addressOnlySupportsPostalCode = addressOnlySupportsPostalCode;
            _geolocationApiKey = geolocationApiKey;

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
                    nativeResponse = ResponseBuilder.TellWithCard(outputSpeech, response.Title ?? String.Empty, JsonConvert.SerializeObject(card), _request.Session);
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
                AuthToken = new Token() { Type = "Bearer", Value = _request.Session?.User?.AccessToken };
            }
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
                    var response = await _addressService.GetAddressAsync(consentToken, baseUri, deviceId, _addressOnlySupportsPostalCode);
                    if (response != null &&
                        (String.IsNullOrEmpty(response.addressLine1) == false ||
                         String.IsNullOrEmpty(response.addressLine2) == false ||
                         String.IsNullOrEmpty(response.addressLine3) == false ||
                         String.IsNullOrEmpty(response.city) == false ||
                         String.IsNullOrEmpty(response.postalCode) == false ||
                         String.IsNullOrEmpty(response.countryCode) == false))
                    {
                        Log("Address retrieved");

                        _address = new Bots.Data.Address()
                        {
                            AddressLine1 = response.addressLine1,
                            AddressLine2 = response.addressLine2,
                            AddressLine3 = response.addressLine3,
                            City = response.city,
                            Postcode = response.postalCode,
                            CountryCode = response.countryCode
                        };
                    }
                    else
                    {
                        Log("Failed to retrieve address");
                    }
                }
                else
                {
                    Log("Device and/or consent token not available");
                }
            }

            return _address;
        }

        public async Task<Tuple<decimal, decimal>> GetLocationAsync()
        {
            if (_location == null)
            {
                if (String.IsNullOrEmpty(_geolocationApiKey))
                {
                    Log("Unable to get location. Geolocation API Key not set.");
                    return null;
                }

                var consentToken = _request.Context?.System?.ApiAccessToken;
                var deviceId = _request.Context?.System?.Device?.DeviceID;
                var baseUri = _request.Context?.System?.ApiEndpoint;

                if (String.IsNullOrEmpty(deviceId) == false &&
                    String.IsNullOrEmpty(consentToken) == false &&
                    String.IsNullOrEmpty(baseUri) == false)
                {
                    var addressResponse = await _addressService.GetAddressAsync(consentToken, baseUri, deviceId, _addressOnlySupportsPostalCode);
                    if (addressResponse != null && String.IsNullOrEmpty(addressResponse.postalCode) == false)
                    {
                        var geoLocationResponse = await _addressService.GetGeoLocationsAsync(_geolocationApiKey, addressResponse.postalCode);
                        if (geoLocationResponse != null)
                        {
                            var result = geoLocationResponse.FirstOrDefault();
                            if (result.geometry?.location != null)
                            {
                                _location = new Tuple<decimal, decimal>(result.geometry.location.lat, result.geometry.location.lng);
                            }
                        }
                    }
                }
            }

            return _location;
        }

        public async Task SendProgressResponseAsync(IBotResponse response)
        {
            var progressiveResponse = new ProgressiveResponse(_request);
            var speechResponse = await progressiveResponse.SendSpeech(response.Speak);
            if (speechResponse.IsSuccessStatusCode)
            {
                Log("Progress Response successful");
            }
            else
            {
                Log("Progress Response unsuccessful");
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
                    specificValue = JsonConvert.DeserializeObject<T>(value.ToString());
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