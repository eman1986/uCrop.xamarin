using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;
using Com.Yalantis.Ucrop;
using Java.IO;
using AndroidUri = Android.Net.Uri;

namespace uCrop.xamarin.demo
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : BaseActivity
    {
        private AndroidUri _photoUri;
        private Button _btnCamera;
        private Button _btnGallery;
        private ImageView _ivResult;

        private readonly string _cacheDir = System.IO.Path.Combine(Application.Context.CacheDir.AbsolutePath, ".tmp");

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            _btnCamera = FindViewById<Button>(Resource.Id.main_btnCamera);
            _btnGallery = FindViewById<Button>(Resource.Id.main_btnGallery);
            _ivResult = FindViewById<ImageView>(Resource.Id.main_ivResult);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case RequestCamera when resultCode == Result.Ok:
                    CropImage();
                    break;
                case RequestGallery when resultCode == Result.Ok && data != null:
                    _photoUri = data.Data;

                    CropImage();
                    break;
                case UCrop.RequestCrop when resultCode == Result.Ok:
                    if (data == null)
                    {
                        return;
                    }

                    var croppedImageUri = UCrop.GetOutput(data);

                    _ivResult.SetImageURI(croppedImageUri);
                    break;
                case UCrop.ResultError:
                case 69:
                    var error = UCrop.GetError(data);

                    Log.Error("error", error.ToString());

                    Toast.MakeText(this, "Failed to crop image.", ToastLength.Long).Show();
                    break;
                default:
                    Toast.MakeText(this, "Request has cancelled.", ToastLength.Long).Show();
                    break;
            }
        }

        protected override void AssignEvents()
        {
            _btnCamera.Click += BtnCameraOnClick;
            _btnGallery.Click += BtnGalleryOnClick;
        }

        protected override void UnassignEvents()
        {
            _btnCamera.Click -= BtnCameraOnClick;
            _btnGallery.Click -= BtnGalleryOnClick;
        }

        private void BtnGalleryOnClick(object sender, EventArgs e)
        {
            ShowGallery();
        }

        private async void BtnCameraOnClick(object sender, EventArgs e)
        {
            await TakePhoto();
        }

        private async Task TakePhoto()
        {
            var permissionGranted = await RequestPermission(Manifest.Permission.Camera, RequestCamera);

            if (!permissionGranted)
            {
                return;
            }

            var tmpFile = CreateTempFile(_cacheDir, "image");

            RunOnUiThread(() =>
            {
                var intent = new Intent(MediaStore.ActionImageCapture);

                if (intent.ResolveActivity(PackageManager) != null)
                {
                    _photoUri = FileProvider.GetUriForFile(this, FileProviderName, tmpFile);

                    intent.PutExtra(MediaStore.ExtraOutput, _photoUri);

                    StartActivityForResult(intent, RequestCamera);
                }
            });
        }

        private void ShowGallery()
        {
            var galleryIntent = new Intent();
            galleryIntent.SetType("image/*");
            galleryIntent.SetAction(Intent.ActionGetContent);

            StartActivityForResult(Intent.CreateChooser(galleryIntent, "Select Picture"), RequestGallery);
        }

        private static File CreateTempFile(string path, string prefix = "")
        {
            var tempDir = new File(path);

            if (!tempDir.Exists())
            {
                tempDir.Mkdirs();
            }

            return File.CreateTempFile(prefix, ".png", tempDir);
        }

        private void CropImage()
        {
            var croppedFile = CreateTempFile(_cacheDir, "cropped");
            var destinationUri = AndroidUri.FromFile(croppedFile);
            var options = new UCrop.Options();

            // applying UI theme
            options.SetToolbarColor(ContextCompat.GetColor(this, Resource.Color.colorPrimary));
            options.SetStatusBarColor(ContextCompat.GetColor(this, Resource.Color.colorPrimary));
            options.SetActiveWidgetColor(ContextCompat.GetColor(this, Resource.Color.colorPrimary));
            options.WithAspectRatio(1, 1);
            options.WithMaxResultSize(2000, 2000);
            options.SetCompressionQuality(80);

            UCrop.Of(_photoUri, destinationUri)
                .WithOptions(options)
                .Start(this);
        }
    }
}