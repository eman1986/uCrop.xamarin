using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Runtime;
using AndroidX.AppCompat.App;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Permission = Android.Content.PM.Permission;

namespace ucrop_native.xamarin.demo
{
    public class BaseActivity : AppCompatActivity
    {
        protected const string FileProviderName = "demo.xamarin.ucrop_native.fileprovider";

        protected const int RequestCamera = 100;

        protected const int RequestGallery = 150;

        private readonly Dictionary<int, TaskCompletionSource<bool>> _permissionTasks = new Dictionary<int, TaskCompletionSource<bool>>();

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            var tcs = _permissionTasks[requestCode];

            if (tcs == null)
            {
                return;
            }

            var denied = grantResults.Contains(Permission.Denied);

            tcs.SetResult(!denied);

            _permissionTasks.Remove(requestCode);
        }

        protected override void OnResume()
        {
            base.OnResume();

            Initialize();

            AssignEvents();

            SetData();
        }

        protected override void OnPause()
        {
            base.OnPause();

            UnassignEvents();
        }

        protected virtual void AssignEvents()
        {
        }

        protected virtual void UnassignEvents()
        {
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void SetData()
        {
        }

        protected async Task<bool> RequestPermission(string permission, int requestCode)
        {
            if (ContextCompat.CheckSelfPermission(this, permission) == Permission.Granted)
            {
                return true;
            }

            if (_permissionTasks.ContainsKey(requestCode))
            {
                return false;
            }

            var tcs = new TaskCompletionSource<bool>();

            _permissionTasks[requestCode] = tcs;

            RunOnUiThread(() =>
            {
                ActivityCompat.RequestPermissions(this, new[] { permission }, requestCode);
            });

            return await tcs.Task;
        }
    }
}
