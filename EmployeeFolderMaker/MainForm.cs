using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EmployeeFolderMaker;

public class MainForm : Form
{
    private TextBox txtFirst, txtLast, txtNid, txtPath;
    private Button btnBrowse, btnCreate;
    private CheckBox chkOpenAfter;

    public MainForm()
    {
        Text = "ساخت پوشه‌های پرسنلی";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 600;
        Height = 320;
        Font = new Font("Segoe UI", 10);
        RightToLeft = RightToLeft.Yes;
        RightToLeftLayout = true;

        var lblFirst = new Label { Text = "نام:", AutoSize = true, Left = 460, Top = 25 };
        txtFirst = new TextBox { Left = 50, Top = 20, Width = 400 };

        var lblLast = new Label { Text = "نام خانوادگی:", AutoSize = true, Left = 460, Top = 65 };
        txtLast = new TextBox { Left = 50, Top = 60, Width = 400 };

        var lblNid = new Label { Text = "کدملی:", AutoSize = true, Left = 460, Top = 105 };
        txtNid = new TextBox { Left = 50, Top = 100, Width = 400 };

        var lblPath = new Label { Text = "مسیر مقصد:", AutoSize = true, Left = 460, Top = 145 };
        txtPath = new TextBox { Left = 50, Top = 140, Width = 320, ReadOnly = true };
        btnBrowse = new Button { Text = "انتخاب مسیر...", Left = 380, Top = 138, Width = 120 };
        btnBrowse.Click += Browse_Click;

        chkOpenAfter = new CheckBox { Text = "پس از ایجاد، پوشه را باز کن", Left = 50, Top = 175, Width = 250, Checked = true };

        btnCreate = new Button { Text = "ایجاد پوشه‌ها", Left = 50, Top = 215, Width = 450, Height = 40 };
        btnCreate.Click += Create_Click;

        Controls.AddRange(new Control[] {
            lblFirst, txtFirst, lblLast, txtLast, lblNid, txtNid, lblPath, txtPath, btnBrowse, chkOpenAfter, btnCreate
        });
    }

    private void Browse_Click(object? sender, EventArgs e)
    {
        using var dlg = new FolderBrowserDialog
        {
            Description = "مسیر مقصد را انتخاب کنید",
            ShowNewFolderButton = true
        };
        if (dlg.ShowDialog(this) == DialogResult.OK)
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

    private void Create_Click(object? sender, EventArgs e)
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
            MessageBox.Show(this, "لطفاً نام، نام خانوادگی، کدملی و مسیر مقصد را وارد کنید.",
                "ورودی ناقص", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // هشدار دوستانه برای کدملی
        if (!Regex.IsMatch(nid, @"^\d{10}$"))
        {
            var r = MessageBox.Show(this, "کدملی ۱۰ رقمی نیست. ادامه بدهم؟",
                "هشدار", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.No) return;
        }

        var rootName = Sanitize($"{first}-{last}-{nid}");
        var target = Path.Combine(path, rootName);

        string[] folders =
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

        try
        {
            Directory.CreateDirectory(target);
            foreach (var f in folders)
                Directory.CreateDirectory(Path.Combine(target, Sanitize(f)));

            MessageBox.Show(this, $"پوشه‌ها ساخته شد:\n{target}",
                "انجام شد", MessageBoxButtons.OK, MessageBoxIcon.Information);

            if (chkOpenAfter.Checked)
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
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
            MessageBox.Show(this, "خطا در ساخت پوشه‌ها:\n" + ex.Message,
                "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
