using Linkly.PosApi.Sdk.DemoPos.Utils;
using Linkly.PosApi.Sdk.Models;
using Linkly.PosApi.Sdk.Models.Authentication;
using Linkly.PosApi.Sdk.Models.ConfigureMerchant;
using Linkly.PosApi.Sdk.Models.Display;
using Linkly.PosApi.Sdk.Models.Logon;
using Linkly.PosApi.Sdk.Models.QueryCard;
using Linkly.PosApi.Sdk.Models.Receipt;
using Linkly.PosApi.Sdk.Models.ReprintReceipt;
using Linkly.PosApi.Sdk.Models.Result;
using Linkly.PosApi.Sdk.Models.SendKey;
using Linkly.PosApi.Sdk.Models.Settlement;
using Linkly.PosApi.Sdk.Models.Status;
using Linkly.PosApi.Sdk.Models.Transaction;
using Linkly.PosApi.Sdk.Service;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using InputType = Linkly.PosApi.Sdk.Models.InputType;

namespace Linkly.PosApi.Sdk.DemoPos.Views
{
    enum Requests { None = -1, Logon, Status, Purchase, Settlement, QueryCard, ReprintReceipt, ConfigureMerchant, Session };
    /// <summary>
    /// Interaction logic for Sessions.xaml
    /// </summary>
    public partial class Sessions : Window, IPosApiEventListener, IDisposable
    {
        private readonly PosApiService _sdk;
        private readonly DataManager _dataManager = new ();
        private readonly ILogger _logger;
        
        private const int RegexTimeoutInMilliseconds = 100;
        
        private ReceiptAutoPrint _receiptAutoPrint;
        private string _secret = string.Empty;
        private Requests _pendingRequest = Requests.None;
        private Button? _activeButton;
        private StackPanel? _activePanel;
        private Dictionary<string, string> _result = new ();
        private Guid _sessionId;
        private bool _isSdkLoggedEnabled = false;
        private List<Pad> _padList = new();
        private List<SurchargeRates> _surcharges = new ();

        public Sessions()
        {
            InitializeComponent();

            _logger = new Logger(LogSdkTrace);
            
            _dataManager.Load();

            _sdk = new PosApiService(_logger, null, this, 
                new ApiServiceEndpoint(new Uri(_dataManager.Current!.AuthEndpoint), new Uri(_dataManager.Current.PosEndpoint)),
                new PosVendorDetails("Linkly Rest Api Pos", Assembly.GetExecutingAssembly().GetName().Version!.ToString(), _dataManager.Sessions.AppGuid, _dataManager.Sessions.AppGuid));

            if (!string.IsNullOrEmpty(_dataManager.Current.Secret))
            {
                _secret = _dataManager.Current.Secret;
                _sdk.SetPairSecret(_secret);
            }
            LogToUi($"Application started. Version {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        public void ConfigureMerchantComplete(Guid sessionId, ConfigureMerchantRequest request, ConfigureMerchantResponse response)
        {
            LogToUi($"ConfigureMerchantComplete: {response.ResponseText}");

            UpdateResult(response);
        }

        public void Display(Guid sessionId, PosApiRequest request, DisplayResponse response)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Display: ");

            foreach (var r in response.DisplayText)
                sb.AppendLine(r);

            LogToUi(sb.ToString(), false);

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                BusyPanel.Visibility = Visibility.Collapsed;
                DisplayPanel.Visibility = Visibility.Visible;

                lblDisplayText1.Content = response.DisplayText[0].Trim();
                lblDisplayText2.Content = response.DisplayText[1].Trim();

                btnDisplay1.Visibility = Visibility.Collapsed;
                btnDisplay2.Visibility = Visibility.Collapsed;

                btnDisplay1.IsEnabled = btnDisplay2.IsEnabled = true;

                if (response.OkKeyFlag)
                {
                    btnDisplay1.Visibility = Visibility.Visible;
                    btnDisplay1.Content = Constants.Ok;
                }
                else if (response.AcceptYesKeyFlag)
                {
                    btnDisplay1.Visibility = Visibility.Visible;
                    btnDisplay1.Content = Constants.Yes;
                }
                else if (response.AuthoriseKeyFlag)
                {
                    btnDisplay1.Visibility = Visibility.Visible;
                    btnDisplay1.Content = Constants.Authorise;
                }

                if (response.CancelKeyFlag)
                {
                    btnDisplay2.Visibility = Visibility.Visible;
                    btnDisplay2.Content = Constants.Cancel;
                }
                else if (response.DeclineNoKeyFlag)
                {
                    btnDisplay2.Visibility = Visibility.Visible;
                    btnDisplay2.Content = Constants.No;
                }

                switch (response.InputType)
                { 
                    case InputType.Normal:
                    case InputType.Decimal:
                        txtDisplayInput.Visibility = Visibility.Visible;
                        txtDisplayAmount.Visibility = Visibility.Collapsed;
                        pbDisplayInput.Visibility = Visibility.Collapsed;
                        break;
                    case InputType.Amount:
                        txtDisplayInput.Visibility = Visibility.Collapsed;
                        txtDisplayAmount.Visibility = Visibility.Visible;
                        pbDisplayInput.Visibility = Visibility.Collapsed;
                        break;
                    case InputType.Password: 
                        txtDisplayInput.Visibility = Visibility.Collapsed;
                        txtDisplayAmount.Visibility = Visibility.Collapsed;
                        pbDisplayInput.Visibility = Visibility.Visible;
                        break;
                    default:
                        txtDisplayInput.Visibility = Visibility.Collapsed;
                        txtDisplayAmount.Visibility = Visibility.Collapsed;
                        pbDisplayInput.Visibility = Visibility.Collapsed;
                        break;
                }

            }));
        }

        public void Error(Guid? sessionId, IBaseRequest? request, ErrorResponse error)
        {
            var message = string.IsNullOrEmpty(error.Message) ? $"Http error {error.HttpStatusCode}!" : $"Http error {error.HttpStatusCode}, {error.Message}!";
            LogToUi($"Error: {message}");
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            if (error.Exception != null)
            {
                LogToUi($"EXCEPTION: {error.Exception.Message}");
                error.Exception = null; // Non-Generic types are not supported by Dictionary used in UpdateResult function
            }
            UpdateResult(error);
            if (_activePanel == PurchasePanel)
                PurchasePanel.Dispatcher.Invoke(new Action(() => PurchasePanel.IsEnabled = true));
        }

        public void LogonComplete(Guid sessionId, LogonRequest request, LogonResponse response)
        {
            LogToUi($"LogonComplete: {response.ResponseText}");
            
            UpdateResult(response);
        }

        private void UpdateResult(object result, bool save = true)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                if (_activeButton != null)
                    _activeButton.IsEnabled = true;

                if (_activePanel != null)
                    _activePanel.IsEnabled = true;

                btnMenuBack.IsEnabled = true;

                _result = Mapping.DictionaryFromType(result);
                dgResult.ItemsSource = _result;

                DisplayPanel.Visibility = Visibility.Collapsed;
                BusyPanel.Visibility = Visibility.Collapsed;

                if (save)
                {
                    if (result is PosApiResponseWithResult response)
                        _dataManager.SaveTransaction(_sessionId, response: response);

                    if (result is ErrorResponse error)
                        _dataManager.SaveTransaction<ErrorResponse>(_sessionId, error: error);
                }
            }));
        }

        public void PairingComplete(PairingRequest request, PairingResponse response)
        {
            LogToUi($"PairingComplete!");

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                StoreSecret(response.Secret);

                PairingPanel.Visibility = Visibility.Collapsed;

                switch (_pendingRequest)
                {
                    case Requests.Logon:
                        LogonPanel.Visibility = Visibility.Visible;
                        _activePanel = LogonPanel;
                        break;
                    case Requests.Status:
                        StatusPanel.Visibility = Visibility.Visible;
                        _activePanel = StatusPanel;
                        break;
                    case Requests.Settlement:
                        SettlementPanel.Visibility = Visibility.Visible;
                        _activePanel = SettlementPanel;
                        break;
                    case Requests.Purchase:
                        PurchasePanel.Visibility = Visibility.Visible;
                        _activePanel = PurchasePanel;
                        break;
                    case Requests.QueryCard:
                        QueryCardPanel.Visibility = Visibility.Visible;
                        _activePanel = QueryCardPanel;
                        break;
                    case Requests.ReprintReceipt:
                        ReprintReceiptPanel.Visibility = Visibility.Visible;
                        _activePanel = ReprintReceiptPanel;
                        break;
                    case Requests.ConfigureMerchant:
                        ConfigureMerchantPanel.Visibility = Visibility.Visible;
                        _activePanel = ConfigureMerchantPanel;
                        break;
                    case Requests.Session:
                        SessionPanel.Visibility = Visibility.Visible;
                        _activePanel = SessionPanel;
                        break;
                    default:
                        MainPanel.Visibility = Visibility.Visible;
                        ContextPanel.Visibility = Visibility.Collapsed;
                        _activePanel = MainPanel;
                        break;
                }

                btnPairPinpad.IsEnabled = true;
                btnMenuBack.IsEnabled = true;
                BusyPanel.Visibility = Visibility.Collapsed;

                txtClientId.Text = txtPassword.Password = txtPairCode.Text = string.Empty;
            }));

            _pendingRequest = Requests.None;
        }

        private void StoreSecret(string secret)
        {
            if (!secret.Equals(_secret))
            {
                _secret = secret;
                _sdk.SetPairSecret(_secret);

                Lane? lane = _dataManager.Sessions.Lanes.FirstOrDefault(x => x.Username.Equals(txtClientId.Text));
                if (lane == default)
                {
                    if (_dataManager.Sessions.Lanes?.Count > 0 && string.IsNullOrEmpty(_dataManager.Sessions.Lanes.FirstOrDefault()?.Username))
                    {
                        lane = _dataManager.Sessions.Lanes.First();
                        lane.Username = txtClientId.Text;
                    }
                    else
                    {
                        lane = new() { Username = txtClientId.Text };
                        _dataManager.Sessions?.Lanes!.Add(lane);
                        lane = _dataManager.Sessions!.Lanes!.LastOrDefault();
                    }
                    cboPairedClientId.Items.Add(txtClientId.Text);
                }

                lane!.AuthEndpoint = txtAuthUrl.Text;
                lane!.PosEndpoint = txtPosUrl.Text;
                lane!.Secret = secret;
                lane!.LastActive = true;

                _dataManager.SaveCurrent(lane);
               
                cboPairedClientId.SelectedValue = txtClientId.Text;
                RefreshSessions(txtClientId.Text);
            }
        }

        public void QueryCardComplete(Guid sessionId, QueryCardRequest request, QueryCardResponse response)
        {
            LogToUi($"QueryCardComplete: {response.ResponseText}");

            UpdateResult(response);
        }

        public void Receipt(Guid sessionId, PosApiRequest request, ReceiptResponse response)
        {
            var sb = new StringBuilder();
            foreach (var r in response.ReceiptText)
                sb.AppendLine(r);

            txtReceipt.Dispatcher.Invoke(() =>
            {
                txtReceipt.Text = sb.ToString();
            });

            sb.Insert(0, $"Receipt: {Environment.NewLine}");
            LogToUi(sb.ToString(), false);
        }

        public void ReprintReceiptComplete(Guid sessionId, ReprintReceiptRequest request, ReprintReceiptResponse response)
        {
            LogToUi($"ReprintReceiptComplete: {response.ResponseText}", false);

            UpdateResult(response);
        }

        public void SettlementComplete(Guid sessionId, SettlementRequest request, SettlementResponse response)
        {
            LogToUi($"SettlementComplete: {response.ResponseText}");

            UpdateResult(response);
        }

        public void StatusComplete(Guid sessionId, StatusRequest request, StatusResponse response)
        {
            LogToUi($"StatusComplete: {response.ResponseText}");
            
            UpdateResult(response);
        }

        public void TransactionComplete(Guid sessionId, TransactionRequest request, TransactionResponse response)
        {
            LogToUi($"TransactionComplete: {response.ResponseText} TxnRef: {response.TxnRef} Amount: {response.Amount} Stan: {response.Stan} RRN: {response.RRN} RFN: {response.RRN}", false);

            UpdateResult(response);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                txtRef.Text = GetReferenceNumber();
                PurchasePanel.IsEnabled = true;
                _ = Enum.TryParse(cboTxnType.Text, out TxnType txnType);
                if (txnType is TxnType.PreAuth)
                    cboRfn.Items.Add(response.RFN);
                if (request.TxnType == TxnType.PreAuthInquiry && !string.IsNullOrEmpty(cboRfn.Text) && response.PurchaseAnalysisData.TryGetValue("PAI", out string? pai))
                    MessageBox.Show("More pre-authorization summary found. Use the `PAI` in the response and send another PreAuthInquiry to get the rest.", 
                        "Additional data found", MessageBoxButton.OK, MessageBoxImage.Information);
            }));
        }

        public void ResultComplete(ResultRequest request, ICollection<PosApiResponse> responses)
        {
            if (responses.Any())
            {
                StringBuilder sb = new();
                sb.AppendLine($"SessionComplete: {request.SessionId}");

                foreach (PosApiResponse response in responses)
                    sb.AppendLine($"{response.ResponseType}");

                sb.AppendLine("Done!");
                LogToUi(sb.ToString());
                UpdateResult(responses.Last(), responses.Last() is PosApiResponseWithResult);
            }
            else
            {
                LogToUi("SessionComplete: empty response result");
            }
        }

        public void RetrieveTransactionComplete(RetrieveTransactionRequest request, ICollection<TransactionResponse> responses)
        {
            // TODO: retrieve transaction by txn ref.
            throw new NotImplementedException();
        }

        private static string GetReferenceNumber() => $"LNK{DateTime.Now:MMddHHmmss}";

        private void btnPairPinpad_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtClientId.Text) || string.IsNullOrEmpty(txtPassword.Password) || string.IsNullOrWhiteSpace(txtPairCode.Text))
            {
                MessageBox.Show("Please provide all data!", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                btnPairPinpad.IsEnabled = false;
                btnMenuBack.IsEnabled = false;
                _activeButton = btnPairPinpad;
                _sdk.PairingRequest(new PairingRequest() { Username = txtClientId.Text, Password = txtPassword.Password, PairCode = txtPairCode.Text });
                BusyPanel.Visibility = Visibility.Visible;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cboReceiptAutoPrint.ItemsSource = Enum.GetValues(typeof(ReceiptAutoPrint)).Cast<ReceiptAutoPrint>();
            cboReceiptAutoPrint.SelectedIndex = 0;
            cboReceiptAutoPrint.SelectedValue = _dataManager.Sessions.ReceiptAutoPrint;
            
            chkCutReceipt.IsChecked = _dataManager.Sessions?.CutReceipt;

            cboTxnType.ItemsSource = Enum.GetValues(typeof(TxnType)).Cast<TxnType>();
            cboTxnType.SelectedIndex = 0;

            cboOrigTxnType.ItemsSource = cboTxnType.ItemsSource;
            cboOrigTxnType.SelectedIndex = -1;

            cboPanSource.ItemsSource = Enum.GetValues(typeof(PanSource)).Cast<PanSource>();
            cboPanSource.SelectedIndex = -1;

            cboAccountType.ItemsSource = Enum.GetValues(typeof(AccountType)).Cast<AccountType>();
            cboAccountType.SelectedIndex = -1;

            cboCurrencyCode.SelectedIndex = 0;

            cboSettlementType.ItemsSource = Enum.GetValues(typeof(SettlementType)).Cast<SettlementType>();
            cboSettlementType.SelectedIndex = 0;

            cboLogonType.ItemsSource = Enum.GetValues(typeof(LogonType)).Cast<LogonType>();
            cboLogonType.SelectedIndex = 0;

            cboQueryCardType.ItemsSource = Enum.GetValues(typeof(QueryCardType)).Cast<QueryCardType>();
            cboQueryCardType.SelectedIndex = 0;

            cboStatusType.ItemsSource = Enum.GetValues(typeof(StatusType)).Cast<StatusType>();
            cboStatusType.SelectedIndex = 0;

            cboSurchargeTypes.ItemsSource = new[] { Constants.Fixed, Constants.Percentage };

            if (!string.IsNullOrEmpty(_dataManager.Current?.AuthEndpoint))
                txtAuthUrl.Text = _dataManager.Current.AuthEndpoint;
            if (!string.IsNullOrEmpty(_dataManager.Current?.PosEndpoint))
                txtPosUrl.Text = _dataManager.Current.PosEndpoint;

            foreach (var lane in _dataManager.Sessions!.Lanes.Where(l => !string.IsNullOrEmpty(l.Username)).Select(x => x.Username))
                cboPairedClientId.Items.Add(lane);

            if (!string.IsNullOrEmpty(_dataManager.Current?.Username))
            {
                cboPairedClientId.SelectedValue = _dataManager.Current?.Username;
                RefreshSessions(_dataManager.Current?.Username!);
            }
            else
            {
                CheckPairing(null, Requests.None);
            }

            if(_dataManager.Sessions?.Pads?.Any() ?? false)
                _padList = _dataManager.Sessions.Pads.ToList();

            dgPads.ItemsSource = _padList;
            dgPadsLogon.ItemsSource = _padList;
            dgPadsSettlement.ItemsSource = _padList;
            dgPadsQuery.ItemsSource = _padList;
            
            if (_dataManager.Sessions?.SurchargeRates?.Any() ?? false)
                _surcharges = _dataManager.Sessions.SurchargeRates.ToList();

            dgSurcharges.ItemsSource = _surcharges;
            dgResult.ItemsSource = new Dictionary<string, string>();

            if (_dataManager.LastSessionInterrupted &&
                MessageBox.Show($"Last transaction interrupted. Would you like to see the result?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                cboSessionId.SelectedValue = _dataManager.LastInterruptedSessionId; // Select last Session
                cboSessionId.IsEnabled = false; // Not allowed to change session id
                SwitchToSessionPage();
                FetchSessionResult();
            }
        }

        private void btnMenuLogon_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Log Menu");
            MainPanel.Visibility = Visibility.Collapsed;
            ContextPanel.Visibility = Visibility.Visible;
            _activePanel = LogonPanel;

            if (string.IsNullOrEmpty(_secret))
            {
                PairingPanel.Visibility = Visibility.Visible;
                btnPairPinpad.IsEnabled = true;
                _pendingRequest = Requests.Logon;
                _activePanel = PairingPanel;
            }
            else
                LogonPanel.Visibility = Visibility.Visible;
        }

        private void btnMenuPairing_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Pairing Menu");
            PairingPanel.Visibility = Visibility.Visible;
            MainPanel.Visibility = Visibility.Collapsed;
            ContextPanel.Visibility = Visibility.Visible;
            _activePanel = PairingPanel;
        }

        private void btnMenuPurchase_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Purchase Menu");
            MainPanel.Visibility = Visibility.Collapsed;
            ContextPanel.Visibility = Visibility.Visible;
            _activePanel = PurchasePanel;
            txtRef.Text = GetReferenceNumber();

            if (string.IsNullOrEmpty(_secret))
            {
                PairingPanel.Visibility = Visibility.Visible;
                btnPairPinpad.IsEnabled = true;
                _pendingRequest = Requests.Purchase;
                _activePanel = PairingPanel;
            }
            else
                PurchasePanel.Visibility = Visibility.Visible;
        }

        private void LogToUi(string messageLine, bool uiThread = false)
        { 
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                string message = $"{DateTime.Now:MM/dd/yyyy hh:mm:ss.fff tt}: {messageLine}";
                int last = 0;

                foreach (var line in message.Split(Environment.NewLine))
                    last = lstLog.Items.Add(line);

                lstLog.SelectedIndex = last;
                lstLog.ScrollIntoView(lstLog.SelectedItem);
            }), uiThread ? System.Windows.Threading.DispatcherPriority.Input : System.Windows.Threading.DispatcherPriority.Background);
        }

        private void btnMenuBack_Click(object sender, RoutedEventArgs e)
        {
            _dataManager.LastSessionInterrupted = false;
            cboSessionId.IsEnabled = true;
            ContextPanel.Visibility = Visibility.Collapsed;
            MainPanel.Visibility = Visibility.Visible;
            if(_activePanel is not null)
                _activePanel.Visibility = Visibility.Collapsed;
        }

        private void btnDisplay1_Click(object sender, RoutedEventArgs e)
        {
            if (btnDisplay1.Content is string content && content.Equals(Constants.Authorise))
            {
                LogToUi($"SendKey ({_sessionId}): {Constants.Authorise}: {Constants.AuthoriseKey}");
                
                string data;
                if (pbDisplayInput.Visibility == Visibility.Visible)
                    data = pbDisplayInput.Text;
                else if (txtDisplayAmount.Visibility == Visibility.Visible)
                    data = txtDisplayAmount.Text;
                else 
                    data = txtDisplayInput.Text;

                _sdk.SendKeyRequest(new SendKeyRequest() { SessionId = _sessionId, Key = Constants.AuthoriseKey, Data = data });
                btnDisplay1.IsEnabled = btnDisplay2.IsEnabled = false;
            }
            else
                SendKey(btnDisplay1, Constants.Yes, Constants.OkCancelKey, Constants.YesKey);
        }
        
        private void btnDisplay2_Click(object sender, RoutedEventArgs e)
        {
            SendKey(btnDisplay2, Constants.No, Constants.OkCancelKey, Constants.NoKey);
        }

        private void SendKey(Button button, string content, string defaultValue, string otherValue)
        {
            string key = button.Content.Equals(content) ? otherValue : defaultValue;
            LogToUi($"SendKey ({_sessionId}): {button.Content}: {key}");
            _sdk.SendKeyRequest(new SendKeyRequest() { SessionId = _sessionId, Key = key });
            btnDisplay1.IsEnabled = btnDisplay2.IsEnabled = false;
        }

        private void btnMenuStatus_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Status Menu");

            CheckPairing(StatusPanel, Requests.Status);
        }

        private void btnMenuSettlement_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Settlement Menu");

            CheckPairing(SettlementPanel, Requests.Settlement);
        }

        private void btnMenuReprintReceipt_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Reprint Receipt Menu");
            
            CheckPairing(ReprintReceiptPanel, Requests.ReprintReceipt);
        }

        private void btnMenuQueryCard_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Query Card Menu");

            CheckPairing(QueryCardPanel, Requests.QueryCard);
        }

        private void btnMenuConfigMerchant_Click(object sender, RoutedEventArgs e)
        {
            LogToUi("Configure Merchant Menu");

            CheckPairing(ConfigureMerchantPanel, Requests.ConfigureMerchant);
        }

        private void SwitchToSessionPage()
        {
            LogToUi("Session Menu");

            CheckPairing(SessionPanel, Requests.Session);
        }

        private void btnMenuSession_Click(object sender, RoutedEventArgs e)
        {
            SwitchToSessionPage();
        }

        private void CheckPairing(StackPanel? panel, Requests request)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            ContextPanel.Visibility = Visibility.Visible;
            if (panel != null)
                _activePanel = panel;

            if (string.IsNullOrEmpty(_secret))
            {
                PairingPanel.Visibility = Visibility.Visible;
                btnPairPinpad.IsEnabled = true;
                _activePanel = PairingPanel;
                _pendingRequest = request;

                if(request is Requests.None)
                    ContextPanel.Visibility = Visibility.Hidden;
            }

            if (panel != null)
                panel.Visibility = Visibility.Visible;
        }

        private void btnLogon_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Logon requested type: {cboLogonType.Text}");
            btnLogon.IsEnabled = false;
            btnMenuBack.IsEnabled = false;

            _ = Enum.TryParse(cboLogonType.Text, out LogonType logonType);

            var request = new LogonRequest { LogonType = logonType, Merchant = txtMerchant.Text, ReceiptAutoPrint = _receiptAutoPrint };
            UpdatePad(request.PurchaseAnalysisData);
            _sessionId = _sdk.LogonRequest(request);

            LogToUi($"Session Id: {_sessionId}");
            _dataManager.SaveTransaction(_sessionId, nameof(LogonRequest), request);
            cboSessionId.Items.Add(_sessionId);
            LogonPanel.IsEnabled = false;
            expLogon.IsExpanded = false;
            _activeButton = btnLogon;
            _activePanel = LogonPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void btnStatus_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Status requested type: {cboStatusType.Text}");
            btnStatus.IsEnabled = false;
            btnMenuBack.IsEnabled = false;
            
            _ = Enum.TryParse(cboStatusType.Text, out StatusType statusType);

            var request = new StatusRequest { StatusType = statusType, Merchant = txtMerchant.Text };
            _sessionId = _sdk.StatusRequest(request);

            LogToUi($"Session Id: {_sessionId}");
            _dataManager.SaveTransaction(_sessionId, nameof(StatusRequest), request);
            cboSessionId.Items.Add(_sessionId);
            StatusPanel.IsEnabled = false;
            _activeButton = btnStatus;
            _activePanel = StatusPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void btnPurchase_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Purchase: {txtRef.Text} Amount: {txtAmount.Text}");

            int amount = txtAmount.Text.ToInt();
            int amountCash = txtAmountCash.Text.ToInt();

            _ = Enum.TryParse(cboTxnType.Text, out TxnType txnType);
            _ = int.TryParse(txtAuthNo.Text, out int authNo);
            _ = int.TryParse(txtTotalCheques.Text, out int totalCheques);
            _ = uint.TryParse(txtPai.Text, out uint pai);

            TransactionRequest? request = txnType switch
            {
                TxnType.Purchase => new PurchaseRequest() { Amount = amount, AmountCash = amountCash },
                TxnType.Refund => new RefundRequest() { Amount = amount },
                TxnType.PreAuth => new PreAuthRequest() { Amount = amount },
                TxnType.PreAuthInquiry => CreatePreAuthInquiryRequest(pai),
                TxnType.PreAuthExtend => new PreAuthExtendRequest() { RFN = cboRfn.Text },
                TxnType.PreAuthTopUp => new PreAuthTopUpRequest() { Amount = amount, RFN = cboRfn.Text },
                TxnType.PreAuthPartialCancel => new PreAuthPartialCancelRequest() { Amount = amount, RFN = cboRfn.Text },
                TxnType.PreAuthCancel => new PreAuthCancelRequest() { RFN = cboRfn.Text },
                TxnType.PreAuthComplete => new PreAuthCompletionRequest() { Amount = amount, RFN = cboRfn.Text },
                TxnType.Cash => new CashRequest() { AmountCash = amountCash },
                TxnType.Deposit => new DepositRequest() { AmountCash = amountCash, AmountCheque = amount, TotalCheques = totalCheques },
                TxnType.Void => new VoidRequest() { Amount = amount },
                _ => null
            };

            if (request is null)
            {
                MessageBox.Show("Not yet implemented!", "Unsupported", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            request.TxnRef = txtRef.Text;
            request.Merchant = txtMerchant.Text;
            request.ReceiptAutoPrint = _receiptAutoPrint;
            request.AuthCode = authNo;
            request.CurrencyCode = cboCurrencyCode.Text;
            request.TrainingMode = chkTrainingMode.IsChecked.GetValueOrDefault();
            request.ProductLevelBlock = chkProductLevelBlock.IsChecked.GetValueOrDefault();

            UpdatePanSource(request);
            UpdatePad(request.PurchaseAnalysisData);

            try
            {
                if (rbTipDefault.IsChecked ?? false)
                    request.EnableTip = true;
                else if (rbTipFixed.IsChecked ?? false)
                    request.TipAmount = txtTipAmount.Text.ToUInt();
                else if (rbTipOptions.IsChecked ?? false)
                    request.SetTipOptions(new TippingOptions(txtTipOption1.Text.ToByte(), txtTipOption2.Text.ToByte(), txtTipOption3.Text.ToByte()));
                
                UpdateSurcharges(request);

                _sessionId = _sdk.TransactionRequest(request);

                LogToUi($"Session Id: {_sessionId}");
                _dataManager.SaveTransaction(_sessionId, nameof(TransactionRequest), request);
                cboSessionId.Items.Add(_sessionId);
                btnPurchase.IsEnabled = false;
                btnMenuBack.IsEnabled = false;
                PurchasePanel.IsEnabled = false;
                _activeButton = btnPurchase;
                _activePanel = PurchasePanel;
                expPurchase.IsExpanded = false;
                BusyPanel.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                LogToUi($"Error: {ex.Message}");
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateSurcharges(TransactionRequest? request)
        {
            if (_surcharges?.Any() ?? false)
            {
                SurchargeOptions surchargeOptions = new();
                foreach (var surcharge in _surcharges)
                {
                    int surchargeValue = surcharge.Value.ToInt();
                    if (surcharge.Type.Equals(Constants.Fixed))
                        surchargeOptions.Add(new FixedSurcharge(surcharge.Bin, surchargeValue));
                    else
                        surchargeOptions.Add(new PercentageSurcharge(surcharge.Bin, surchargeValue));
                }

                request?.SetSurchargeOptions(surchargeOptions);
            }
        }

        private void UpdatePanSource(TransactionRequest request)
        {
            if (cboPanSource.SelectedIndex != -1 && Enum.TryParse(cboPanSource.Text, out PanSource panSource))
            {
                request.PanSource = panSource;
                if (panSource != PanSource.PinPad)
                {
                    _ = Enum.TryParse(cboAccountType.Text, out AccountType accountType);
                    request.AccountType = accountType;

                    if (panSource != PanSource.PosSwiped || panSource != PanSource.PinPad)
                    {
                        request.PAN = txtPan.Text;
                        request.DateExpiry = txtExpiry.Text;
                    }
                    else
                        request.Track2 = txtTrack2.Text;
                }
            }
        }

        private TransactionRequest CreatePreAuthInquiryRequest(uint pai)
        {
            if (string.IsNullOrWhiteSpace(cboRfn.Text))
            {
                PreAuthSummaryRequest request = new();
                if (pai > 0)
                    request.PreAuthIndex = pai;

                return request;
            }
            else
                return new PreAuthInquiryRequest() { RFN = cboRfn.Text };
        }

        private void UpdatePad(IDictionary<string, string> requestPad)
        {
            foreach (var pad in _padList.Where(x => x.Use && !string.IsNullOrWhiteSpace(x.Tag) && !string.IsNullOrWhiteSpace(x.Value)))
                requestPad.Add(pad.Tag!, pad.Value!);
        }

        private void btnSettlement_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Settlement requested type: {cboSettlementType.Text}");
            
            _ = Enum.TryParse(cboSettlementType.Text, out SettlementType settlementType);

            SettlementRequest request = settlementType switch
            {
                SettlementType.SubShiftTotals => new SettlementRequest { SettlementType = SettlementType.SubShiftTotals , ResetTotals = chkResetTotals.IsChecked ?? false, ReceiptAutoPrint = _receiptAutoPrint, Merchant = txtMerchant.Text },
                _ => new SettlementRequest { SettlementType = settlementType, ReceiptAutoPrint = _receiptAutoPrint, Merchant = txtMerchant.Text }
            };

            UpdatePad(request.PurchaseAnalysisData);
            _sessionId = _sdk.SettlementRequest(request);

            LogToUi($"Session Id: {_sessionId}");
            _dataManager.SaveTransaction(_sessionId, nameof(SettlementRequest), request);
            cboSessionId.Items.Add(_sessionId);

            btnSettlement.IsEnabled = false;
            btnMenuBack.IsEnabled = false;
            SettlementPanel.IsEnabled = false;
            expSettlement.IsExpanded = false;
            _activeButton = btnSettlement;
            _activePanel = SettlementPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void btnQueryCard_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Query Card requested type: {cboQueryCardType.Text}");

            _ = Enum.TryParse(cboQueryCardType.Text, out QueryCardType queryCardType);

            var request = new QueryCardRequest { QueryCardType = queryCardType, Merchant = txtMerchant.Text };
            UpdatePad(request.PurchaseAnalysisData);
            _sessionId = _sdk.QueryCardRequest(request);

            LogToUi($"Session Id: {_sessionId}");
            _dataManager.SaveTransaction(_sessionId, nameof(QueryCardRequest), request);
            cboSessionId.Items.Add(_sessionId);

            btnQueryCard.IsEnabled = false;
            btnMenuBack.IsEnabled = false;
            QueryCardPanel.IsEnabled = false;
            expQuery.IsExpanded = false;
            _activeButton = btnQueryCard;
            _activePanel = QueryCardPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void btnReprintReceipt_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Reprint receipt");

            var request = new ReprintReceiptRequest { ReprintType = ReprintType.Reprint, Merchant = txtMerchant.Text, CutReceipt = chkCutReceipt.IsChecked ?? false, ReceiptAutoPrint = _receiptAutoPrint };
            _sessionId = _sdk.ReprintReceiptRequest(request);

            LogToUi($"Session Id: {_sessionId}");
            _dataManager.SaveTransaction(_sessionId, nameof(ReprintReceiptRequest), request);
            cboSessionId.Items.Add(_sessionId);

            btnMenuBack.IsEnabled = false;
            btnReprintReceipt.IsEnabled = false;
            ReprintReceiptPanel.IsEnabled = false;
            _activeButton = btnReprintReceipt;
            _activePanel = ReprintReceiptPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void btnConfigureMerchant_Click(object sender, RoutedEventArgs e)
        {
            LogToUi($"Configure Merchant");

            var request = new ConfigureMerchantRequest { CaId = txtCaId.Text, CatId = txtCatId.Text };
            _sessionId = _sdk.ConfigureMerchantRequest(request);

            LogToUi($"Session Id: {_sessionId}");
            _dataManager.SaveTransaction(_sessionId, nameof(ConfigureMerchantRequest), request);
            cboSessionId.Items.Add(_sessionId);

            btnConfigureMerchant.IsEnabled = false;
            ConfigureMerchantPanel.IsEnabled = false;
            _activeButton = btnConfigureMerchant;
            _activePanel = ConfigureMerchantPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void FetchSessionResult()
        {
            LogToUi($"Session: {cboSessionId.Text}");
            _sessionId = Guid.Empty;

            _ = Guid.TryParse(cboSessionId.Text, out _sessionId);

            var request = new ResultRequest(_sessionId);
            _sdk.ResultRequest(request);

            btnSession.IsEnabled = false;
            SessionPanel.IsEnabled = false;
            _activeButton = btnSession;
            _activePanel = SessionPanel;
            BusyPanel.Visibility = Visibility.Visible;
        }

        private void btnSession_Click(object sender, RoutedEventArgs e)
        {
            FetchSessionResult();
        }

        public void LogSdkTrace(string message)
        {
            if(_isSdkLoggedEnabled)
                LogToUi($"SDK: {message}");
        }

        private void btnSaveLog_Click(object sender, RoutedEventArgs e)
        {
            if (lstLog is not null && lstLog.Items is not null)
            {
                DirectoryInfo directoryInfo = Directory.CreateDirectory("logs");
                string filename = Path.Combine(directoryInfo.FullName, $"log-{DateTime.Now:ddMMyy-mmss}.txt");
                var sb = new StringBuilder();
                foreach (var item in lstLog.Items)
                    sb.AppendLine(item as string);

                File.WriteAllText(filename, sb.ToString());
                System.Diagnostics.Process.Start(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe"), $"/select, {filename}");
            }
        }

        private void chkEnableSdkLogging_Checked(object sender, RoutedEventArgs e)
        {
            _isSdkLoggedEnabled = chkEnableSdkLogging.IsChecked ?? false;
        }

        private void btnClearLog_Click(object sender, RoutedEventArgs e)
        {
            lstLog.Items.Clear();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            bool save = false;
            if (_padList?.Any() ?? false)
            {
                _dataManager.Sessions.Pads = _padList;
                save = true;
            }

            if (_surcharges?.Any() ?? false)
            {
                _dataManager.Sessions.SurchargeRates = _surcharges;
                save = true;
            }

            if (save)
                _dataManager.Save();

            _dataManager.Dispose();
        }

        private void cboSettlementType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboSettlementType.SelectedIndex > -1)
            {
                var st = cboSettlementType.Items[cboSettlementType.SelectedIndex].ToString();
                _ = Enum.TryParse(st, out SettlementType settlementType);
                chkResetTotals.Visibility = (settlementType == SettlementType.SubShiftTotals) ? Visibility.Visible : Visibility.Hidden;
            }

        }

        private void cboReceiptAutoPrint_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboReceiptAutoPrint.SelectedIndex > -1)
            {
                var st = cboReceiptAutoPrint.Items[cboReceiptAutoPrint.SelectedIndex].ToString();
                _ = Enum.TryParse(st, out ReceiptAutoPrint receiptAutoPrint);
                _receiptAutoPrint = receiptAutoPrint;
            }
        }
        
        private void RefreshSessions(string username)
        {
            LogToUi($"Client Id: {username}");
            lblUsername.Content = username;

            cboSessionId.Items.Clear();
            cboRfn.Items.Clear();
            foreach (var session in _dataManager.Current!.Transactions)
            {
                cboSessionId.Items.Add(session.SessionId);
                if (session.Response is TransactionResponse response && !string.IsNullOrEmpty(response.RFN))
                    cboRfn.Items.Add(response.RFN);
            }
        }

        private void cboPairedClientId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cboPairedClientId.SelectedIndex == -1)
                return;

            var lane = _dataManager.Sessions.Lanes.FirstOrDefault(x => x.Username.Equals(cboPairedClientId.Items[cboPairedClientId.SelectedIndex].ToString()));
            if (lane != default && lane != _dataManager.Current)
            {
                _secret = lane.Secret;
                _sdk.SetPairSecret(_secret);
                _dataManager.SaveCurrent(lane);

                RefreshSessions(lane.Username);
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = Regex.IsMatch(e.Text, "[^0-9]+", RegexOptions.None, TimeSpan.FromMilliseconds(RegexTimeoutInMilliseconds));
        }

        private void cboPanSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboPanSource.SelectedIndex == -1)
                return;

            _ = Enum.TryParse(cboPanSource.Text, out PanSource panSource);

            switch (panSource)
            {
                case PanSource.PosKeyed:
                case PanSource.TeleOrder:
                case PanSource.Internet:
                case PanSource.Installment:
                case PanSource.CustomerPresent:
                case PanSource.Moto:
                case PanSource.RecurringTransaction:
                    panelPanManualEntry.Visibility = Visibility.Visible;
                    panelPanAccountType.Visibility = Visibility.Visible;
                    panelPanSwiped.Visibility = Visibility.Collapsed;
                    break;
                case PanSource.PosSwiped:
                    panelPanSwiped.Visibility = Visibility.Visible;
                    panelPanAccountType.Visibility = Visibility.Visible;
                    panelPanManualEntry.Visibility = Visibility.Collapsed;
                    break;
                default:
                    panelPanSwiped.Visibility = Visibility.Collapsed;
                    panelPanAccountType.Visibility = Visibility.Collapsed;
                    panelPanManualEntry.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void cboTxnType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboTxnType.SelectedIndex == -1)
                return;

            _ = Enum.TryParse(cboTxnType.Items[cboTxnType.SelectedIndex].ToString(), out TxnType txnType);

            panelRfn.Visibility = (txnType == TxnType.PreAuthExtend || txnType == TxnType.PreAuthTopUp || txnType == TxnType.PreAuthCancel || txnType == TxnType.PreAuthInquiry || txnType == TxnType.PreAuthComplete || txnType == TxnType.PreAuthPartialCancel || txnType == TxnType.Refund) ? Visibility.Visible : Visibility.Collapsed;
            panelAmountCash.Visibility = (txnType == TxnType.Purchase || txnType == TxnType.Cash || txnType == TxnType.Deposit) ? Visibility.Visible : Visibility.Collapsed;
            panelCheques.Visibility = txnType == TxnType.Deposit ? Visibility.Visible : Visibility.Collapsed;
            panelAmount.Visibility = (txnType == TxnType.Purchase || txnType == TxnType.Refund || txnType == TxnType.Deposit || txnType == TxnType.PreAuthTopUp || txnType == TxnType.PreAuthPartialCancel || txnType == TxnType.PreAuthComplete || txnType == TxnType.PreAuth || txnType == TxnType.Void) ? Visibility.Visible : Visibility.Collapsed;
            panelPai.Visibility = (txnType == TxnType.PreAuthInquiry) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Dispose();
        }

        private void cboPairedClientId_KeyUp(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Delete || e.Key == Key.Back) &&
                MessageBox.Show($"Are you sure you want to delete {cboPairedClientId.Text} from the list of paired accounts?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes)
            {
                int index = cboPairedClientId.SelectedIndex;
                _dataManager.Sessions.Lanes.Remove(_dataManager.Current!);
                cboPairedClientId.Items.Remove(cboPairedClientId.Items[index]);
                if (cboPairedClientId.Items.Count == 0)
                {
                    _secret = string.Empty;
                    CheckPairing(null, Requests.None);
                }
                else
                    cboPairedClientId.SelectedIndex = index > cboPairedClientId.Items.Count - 1 ? cboPairedClientId.Items.Count - 1 : index;
            }
        }

        private void rbTipDefault_Checked(object sender, RoutedEventArgs e)
        {
            ShowTipping(rbTipDefault.Name);
        }

        private void rbTipNone_Checked(object sender, RoutedEventArgs e)
        {
            ShowTipping(rbTipNone.Name);
        }

        private void rbTipOptions_Checked(object sender, RoutedEventArgs e)
        {
            ShowTipping(rbTipOptions.Name);
        }

        private void rbTipFixed_Checked(object sender, RoutedEventArgs e)
        {
            ShowTipping(rbTipFixed.Name);
        }

        private void ShowTipping(string radioButton)
        {
            if (radioButton == null || panelTipAmount == null || panelTipOptions == null)
                return;

            switch (radioButton)
            {
                case nameof(rbTipOptions):
                    panelTipAmount.Visibility = Visibility.Collapsed;
                    panelTipOptions.Visibility = Visibility.Visible;
                    break;
                case nameof(rbTipFixed):
                    panelTipAmount.Visibility = Visibility.Visible;
                    panelTipOptions.Visibility = Visibility.Collapsed;
                    break;
                default:
                    panelTipAmount.Visibility = Visibility.Collapsed;
                    panelTipOptions.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        
    }

}
