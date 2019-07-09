using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Content.PM;
using Android.Telephony;
using System;
using Android;
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V4.App;
using Android.Util;
using Android.Support.Design.Widget;
using Android.Views;

namespace App24
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback 
    {
        static readonly int REQUEST_SENDSMS = 0;

        public  string TAG
        {
            get
            {
                return "MainActivity";
            }
        }
        static string[] PERMISSIONS_SENDMSG = {
            Manifest.Permission.SendSms,
            Manifest.Permission.ReadPhoneState
        };

        View layout;
        private SmsManager _smsManager;
        private BroadcastReceiver _smsSentBroadcastReceiver, _smsDeliveredBroadcastReceiver;

        Button smsBtn;
        EditText phoneNum;
        EditText sms;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.layout1);
            layout = FindViewById(Resource.Id.sample_main_layout);
            smsBtn = FindViewById<Button>(Resource.Id.btnSend);
            phoneNum = FindViewById<EditText>(Resource.Id.phoneNum);
            sms = FindViewById<EditText>(Resource.Id.txtSMS);
            _smsManager = SmsManager.Default;

            smsBtn.Click += (s, e) =>
            {
                checkSendMsgPermission();

            };
        }

        void checkSendMsgPermission() {
            Log.Info(TAG, "button pressed. Checking permissions.");

            // Verify that all required  permissions have been granted.
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.SendSms) != (int)Permission.Granted
                || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadPhoneState) != (int)Permission.Granted)
            {
                // Contacts permissions have not been granted.
                Log.Info(TAG, " permissions has NOT been granted. Requesting permissions.");
                RequestSendMsgPermissions();
            }
            else
            {
                //  permissions have been granted. 
                Log.Info(TAG, " permissions have already been granted.");

                var phone = phoneNum.Text;
                var message = sms.Text;
                var piSent = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
                var piDelivered = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);
                _smsManager.SendTextMessage(phone, null, message, piSent, piDelivered);
            }
        }

        private void RequestSendMsgPermissions()
        {

            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.SendSms)
                || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadPhoneState))
            {

                // Display a SnackBar with an explanation and a button to trigger the request.
                Snackbar.Make(layout, "Message permission is needed to send SMS.",
                    Snackbar.LengthIndefinite).SetAction("OK", new Action<View>(delegate (View obj) {
                        ActivityCompat.RequestPermissions(this, PERMISSIONS_SENDMSG, REQUEST_SENDSMS);
                    })).Show();
            }
            else
            {
                // Contact permissions have not been granted yet. Request them directly.
                ActivityCompat.RequestPermissions(this, PERMISSIONS_SENDMSG, REQUEST_SENDSMS);
            }

        }


        protected override void OnResume()
        {
            base.OnResume();

            _smsSentBroadcastReceiver = new SMSSentReceiver();
            _smsDeliveredBroadcastReceiver = new SMSDeliveredReceiver();

            RegisterReceiver(_smsSentBroadcastReceiver, new IntentFilter("SMS_SENT"));
            RegisterReceiver(_smsDeliveredBroadcastReceiver, new IntentFilter("SMS_DELIVERED"));
        }

        protected override void OnPause()
        {
            base.OnPause();

            UnregisterReceiver(_smsSentBroadcastReceiver);
            UnregisterReceiver(_smsDeliveredBroadcastReceiver);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            //Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode== REQUEST_SENDSMS) {
                if (PermissionUtil.VerifyPermissions(grantResults))
                {
                    // All required permissions have been granted, display contacts fragment.
                    Snackbar.Make(layout, " Permissions have been granted. ", Snackbar.LengthShort).Show();
                    var phone = phoneNum.Text;
                    var message = sms.Text;
                    var piSent = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_SENT"), 0);
                    var piDelivered = PendingIntent.GetBroadcast(this, 0, new Intent("SMS_DELIVERED"), 0);
                    _smsManager.SendTextMessage(phone, null, message, piSent, piDelivered);

                }
                else
                {
                    Log.Info(TAG, " permissions were NOT granted.");
                    Snackbar.Make(layout, "Permissions were not granted.", Snackbar.LengthShort).Show();
                }
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        
    }







    [BroadcastReceiver]
    public class SMSSentReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            switch ((int)ResultCode)
            {
                case (int)Result.Ok:
                    Toast.MakeText(Application.Context, "SMS has been sent", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.GenericFailure:
                    Toast.MakeText(Application.Context, "Generic Failure", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.NoService:
                    Toast.MakeText(Application.Context, "No Service", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.NullPdu:
                    Toast.MakeText(Application.Context, "Null PDU", ToastLength.Short).Show();
                    break;
                case (int)SmsResultError.RadioOff:
                    Toast.MakeText(Application.Context, "Radio Off", ToastLength.Short).Show();
                    break;
            }
        }
    }

    [BroadcastReceiver]
    public class SMSDeliveredReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            switch ((int)ResultCode)
            {
                case (int)Result.Ok:
                    Toast.MakeText(Application.Context, "SMS Delivered", ToastLength.Short).Show();
                    break;
                case (int)Result.Canceled:
                    Toast.MakeText(Application.Context, "SMS not delivered", ToastLength.Short).Show();
                    break;
            }
        }           

        
    }
}