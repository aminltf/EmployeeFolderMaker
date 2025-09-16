using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using Forms = System.Windows.Forms;

namespace EmployeeFolderMaker.WpfApp;

public partial class MainWindow : Window
{
    // پوشه‌های حالت «قراردادی/رسمی»
    private static readonly string[] Folders_ContractOfficial =
    {
        "ابقاء و رهایی",
        "اطلاعات پرسنلی",
        "امور بیمه و وام",
        "ایثارگری",
        "آموزش",
        "ترفیعات و انتصابات",
        "تشویق و تنبیه",
        "کمیسیون",
        "متفرقه",
        "مرخصی و ماموریت",
        "نقل و انقالات"
    };

    // پوشه‌های حالت «سرباز وظیفه»
    private static readonly string[] Folders_Conscripts =
    {
        "تسویه حساب",
        "مرخصی",
        "تشویق",
        "تنبیه",
        "اضافه خدمت",
        "کسر خدمت",
        "عائله مندی",
        "انتقالات",
        "مدرک تحصیلی",
        "آموزش"
    };

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        using var dlg = new Forms.FolderBrowserDialog
        {
            Description = "مسیر مقصد را انتخاب کنید",
            ShowNewFolderButton = true
        };
        if (dlg.ShowDialog() == Forms.DialogResult.OK)
        {
            txtPath.Text = dlg.SelectedPath;
        }
    }

    private static string Sanitize(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '-');
        return name.Trim();
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        var first = txtFirst.Text.Trim();
        var last = txtLast.Text.Trim();
        var nid = txtNid.Text.Trim();
        var path = txtPath.Text.Trim();

        if (string.IsNullOrWhiteSpace(first) ||
            string.IsNullOrWhiteSpace(last) ||
            string.IsNullOrWhiteSpace(nid) ||
            string.IsNullOrWhiteSpace(path))
        {
            System.Windows.MessageBox.Show(this, "لطفاً نام، نام خانوادگی، کدملی و مسیر مقصد را کامل وارد کنید.",
                "ورودی ناقص", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!Regex.IsMatch(nid, @"^\d{10}$"))
        {
            var r = System.Windows.MessageBox.Show(this, "کدملی ۱۰ رقمی نیست. ادامه بدهم؟",
                "هشدار", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.No) return;
        }

        if (!Directory.Exists(path))
        {
            var r = System.Windows.MessageBox.Show(this, "مسیر مقصد وجود ندارد. ایجاد شود؟",
                "مسیر نامعتبر", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r == MessageBoxResult.No) return;
            try { Directory.CreateDirectory(path); }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this, "عدم امکان ایجاد مسیر:\n" + ex.Message,
                    "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        // تشخیص نوع عضویت
        var selected = (cmbMembership.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "رسمی";
        var isConscript = selected == "سرباز وظیفه";

        var membershipName = (cmbMembership.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content?.ToString() ?? "رسمی";
        var rootName = Sanitize($"{first}-{last}-{nid}-{membershipName}");
        var target = Path.Combine(path, rootName);

        var folders = isConscript ? Folders_Conscripts : Folders_ContractOfficial;

        try
        {
            Directory.CreateDirectory(target);
            foreach (var f in folders)
                Directory.CreateDirectory(Path.Combine(target, Sanitize(f)));

            var kind = isConscript ? "سرباز وظیفه" : selected; // برای پیام
            System.Windows.MessageBox.Show(this, $"پوشه‌ها ({kind}) ساخته شد:\n{target}",
                "انجام شد", MessageBoxButton.OK, MessageBoxImage.Information);

            if (chkOpenAfter.IsChecked == true)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    });
                }
                catch { /* no-op */ }
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(this, "خطا در ساخت پوشه‌ها:\n" + ex.Message,
                "خطا", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
