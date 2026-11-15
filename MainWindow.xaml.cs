using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Autokolcsonzo
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            KezdetiAdatokBetoltese();
            AdatokFrissitese();
        }

        private Dolgozo aktualisDolgozo;
        private bool kijelentkezesTortent = false;

        public MainWindow(Dolgozo belepve)
        {
            InitializeComponent();
            aktualisDolgozo = belepve;
            tbBelepveInfo.Text = $"Felhasználó: {belepve.Nev} | Beosztás: {belepve.Beosztas}";

            KezdetiAdatokBetoltese();
            JogosultsagokBeallitasa();
            AdatokFrissitese();
        }

        private void JogosultsagokBeallitasa()
        {
            if (aktualisDolgozo.Beosztas == "ugyintezo")
            {
                
                spAutoSzerkeszto.IsEnabled = false;
                tbStatisztika.Visibility = Visibility.Hidden;
                tcFoMenu.SelectedItem = tabKolcsonzesek;
            }
            else if (aktualisDolgozo.Beosztas == "szerelo")
            {
                
                btnAutoTorol.IsEnabled = false;
                tabUgyfelek.Visibility = Visibility.Collapsed;
                tabKolcsonzesek.Visibility = Visibility.Collapsed;
            }
        }

        private void btnKijelentkezes_Click(object sender, RoutedEventArgs e)
        {
            kijelentkezesTortent = true;
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
        private void KezdetiAdatokBetoltese()
        {
            using (var cn = new cnAutokolcsonzo())
            {
                try
                {
                    cn.Database.EnsureCreated();
                    if (!cn.Autok.Any())
                    {
                        var a1 = new Auto { Rendszam = "AA-AA-123", MarkaTipus = "Toyota Corolla", Kategoria = "Személyautó", NapiDij = 15000, Elerheto = true };
                        var a2 = new Auto { Rendszam = "BB-BB-456", MarkaTipus = "Kia Sportage", Kategoria = "SUV", NapiDij = 22000, Elerheto = false };
                        var u1 = new Ugyfel { Nev = "Kovács János", JogosimvanySzam = "AB123456", Telefon = "06301112233" };
                        var u2 = new Ugyfel { Nev = "Nagy Éva", JogosimvanySzam = "CD654321", Telefon = "06704445566" };

                        cn.Autok.AddRange(a1, a2);
                        cn.Ugyfelek.AddRange(u1, u2);
                        cn.Kolcsonzesek.Add(new Kolcsonzes { Auto = a2, Ugyfel = u1, Kezdete = DateTime.Today.AddDays(-3), Allapot = "Aktív", FizetendoOsszeg = 0 });
                        cn.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Adatbázis hiba: " + ex.Message);
                }
            }
        }

        private void AdatokFrissitese()
        {
            using (var cn = new cnAutokolcsonzo())
            {
                dgAutok.ItemsSource = cn.Autok.ToList();
                dgUgyfelek.ItemsSource = cn.Ugyfelek.ToList();
                dgKolcsonzesek.ItemsSource = cn.Kolcsonzesek.Include(k => k.Auto).Include(k => k.Ugyfel).ToList();

                cbKolcsonAuto.ItemsSource = cn.Autok.Where(a => a.Elerheto).ToList();
                cbKolcsonUgyfel.ItemsSource = cn.Ugyfelek.ToList();

                int osszBevetel = cn.Kolcsonzesek.Where(k => k.Allapot == "Lezárt").Sum(k => (int?)k.FizetendoOsszeg) ?? 0;
                int aktivDb = cn.Kolcsonzesek.Count(k => k.Allapot == "Aktív");
                tbStatisztika.Text = $"Aktív kölcsönzések: {aktivDb} db | Összes bevétel: {osszBevetel} Ft";
            }
        }

        private void tbAutoKereses_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (var cn = new cnAutokolcsonzo())
            {
                string kereso = tbAutoKereses.Text.Trim().ToLower();
                dgAutok.ItemsSource = cn.Autok
                    .Where(a => a.MarkaTipus.ToLower().Contains(kereso) || a.Rendszam.ToLower().Contains(kereso))
                    .ToList();
            }
        }

        private void tbUgyfelKereses_TextChanged(object sender, TextChangedEventArgs e)
        {
            using (var cn = new cnAutokolcsonzo())
            {
                string kereso = tbUgyfelKereses.Text.Trim().ToLower();
                dgUgyfelek.ItemsSource = cn.Ugyfelek
                    .Where(u => u.Nev.ToLower().Contains(kereso) || u.JogosimvanySzam.ToLower().Contains(kereso))
                    .ToList();
            }
        }
       
        private void dgAutok_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAutok.SelectedItem is Auto a)
            {
                tbRendszam.Text = a.Rendszam;
                tbMarka.Text = a.MarkaTipus;
                cbKategoria.Text = a.Kategoria;
                tbNapiDij.Text = a.NapiDij.ToString();
                chkElerheto.IsChecked = a.Elerheto;
            }
        }

        private void btnAutoHozzaad_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbNapiDij.Text, out int dij) || string.IsNullOrWhiteSpace(tbRendszam.Text)) return;
            using (var cn = new cnAutokolcsonzo())
            {
                cn.Autok.Add(new Auto
                {
                    Rendszam = tbRendszam.Text,
                    MarkaTipus = tbMarka.Text,
                    Kategoria = cbKategoria.Text,
                    NapiDij = dij,
                    Elerheto = chkElerheto.IsChecked == true
                });
                cn.SaveChanges();
            }
            AdatokFrissitese();
        }

        private void btnAutoModosit_Click(object sender, RoutedEventArgs e)
        {
            if (dgAutok.SelectedItem is not Auto kijelolt || !int.TryParse(tbNapiDij.Text, out int dij)) return;
            using (var cn = new cnAutokolcsonzo())
            {
                var a = cn.Autok.FirstOrDefault(x => x.Id == kijelolt.Id);
                if (a != null)
                {
                    a.Rendszam = tbRendszam.Text;
                    a.MarkaTipus = tbMarka.Text;
                    a.Kategoria = cbKategoria.Text;
                    a.NapiDij = dij;
                    a.Elerheto = chkElerheto.IsChecked ?? true;
                    cn.SaveChanges();
                }
            }
            AdatokFrissitese();
        }

        private void btnAutoTorol_Click(object sender, RoutedEventArgs e)
        {
            Auto kijelolt = dgAutok.SelectedItem as Auto;
            if (kijelolt == null) return;
            using (var cn = new cnAutokolcsonzo())
            {
                var a = cn.Autok.FirstOrDefault(x => x.Id == kijelolt.Id);
                if (a != null)
                {
                    cn.Autok.Remove(a);
                    cn.SaveChanges();
                }
            }
            AdatokFrissitese();
        }

       
        private void dgUgyfelek_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgUgyfelek.SelectedItem is Ugyfel u)
            {
                tbUgyfelNev.Text = u.Nev;
                tbJogsi.Text = u.JogosimvanySzam;
                tbTelefon.Text = u.Telefon;
            }
        }

        private void btnUgyfelHozzaad_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUgyfelNev.Text)) return;
            using (var cn = new cnAutokolcsonzo())
            {
                cn.Ugyfelek.Add(new Ugyfel { Nev = tbUgyfelNev.Text, JogosimvanySzam = tbJogsi.Text, Telefon = tbTelefon.Text });
                cn.SaveChanges();
            }
            AdatokFrissitese();
        }

        private void btnUgyfelModosit_Click(object sender, RoutedEventArgs e)
        {
            if (dgUgyfelek.SelectedItem is not Ugyfel kijelolt) return;
            using (var cn = new cnAutokolcsonzo())
            {
                var u = cn.Ugyfelek.FirstOrDefault(x => x.Id == kijelolt.Id);
                if (u != null)
                {
                    u.Nev = tbUgyfelNev.Text;
                    u.JogosimvanySzam = tbJogsi.Text;
                    u.Telefon = tbTelefon.Text;
                    cn.SaveChanges();
                }
            }
            AdatokFrissitese();
        }

        private void btnUgyfelTorol_Click(object sender, RoutedEventArgs e)
        {
            if (dgUgyfelek.SelectedItem is not Ugyfel kijelolt) return;
            using (var cn = new cnAutokolcsonzo())
            {
                var u = cn.Ugyfelek.FirstOrDefault(x => x.Id == kijelolt.Id);
                if (u != null)
                {
                    cn.Ugyfelek.Remove(u);
                    cn.SaveChanges();
                }
            }
            AdatokFrissitese();
        }

        // Kölcsönzések
        private void btnKolcsonzesIndit_Click(object sender, RoutedEventArgs e)
        {
            if (cbKolcsonAuto.SelectedItem is not Auto auto || cbKolcsonUgyfel.SelectedItem is not Ugyfel ugyfel) return;
            using (var cn = new cnAutokolcsonzo())
            {
                var dbAuto = cn.Autok.FirstOrDefault(a => a.Id == auto.Id);
                if (dbAuto != null)
                {
                    dbAuto.Elerheto = false;
                    cn.Kolcsonzesek.Add(new Kolcsonzes
                    {
                        AutoId = dbAuto.Id,
                        UgyfelId = ugyfel.Id,
                        Kezdete = DateTime.Today,
                        Allapot = "Aktív",
                        FizetendoOsszeg = 0
                    });
                    cn.SaveChanges();
                }
            }
            AdatokFrissitese();
        }

        private void btnVisszavetel_Click(object sender, RoutedEventArgs e)
        {
            if (dgKolcsonzesek.SelectedItem is not Kolcsonzes kijelolt || kijelolt.Allapot == "Lezárt") return;
            using (var cn = new cnAutokolcsonzo())
            {
                var k = cn.Kolcsonzesek.Include(x => x.Auto).Include(x => x.Ugyfel).FirstOrDefault(x => x.Id == kijelolt.Id);
                if (k != null)
                {
                    k.Vege = DateTime.Today;
                    int napok = (k.Vege.Value - k.Kezdete).Days;
                    if (napok < 1) napok = 1; 
                    k.FizetendoOsszeg = napok * k.Auto.NapiDij;
                    k.Allapot = "Lezárt";
                    k.Auto.Elerheto = true;
                    cn.SaveChanges();

                    MessageBox.Show($"SZÁMLA\n\nÜgyfél: {k.Ugyfel.Nev}\nAutó: {k.Auto.MarkaTipus} ({k.Auto.Rendszam})\nNapok száma: {napok} nap\nFizetendő végösszeg: {k.FizetendoOsszeg} Ft", "Számla elkészült");
                }
            }
            AdatokFrissitese();
        }

        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (kijelentkezesTortent) return;
            try
            {
                using (var cn = new cnAutokolcsonzo())
                {
                    var kapcsolat = cn.Database.GetDbConnection();
                    kapcsolat.Open();
                    var db = kapcsolat.Database;
                    kapcsolat.Close();
                    SqlConnection.ClearAllPools();

                    var mastercs = new SqlConnectionStringBuilder(cnAutokolcsonzo.GetConnectionString("csAutokolcsonzo"))
                    {
                        InitialCatalog = "master",
                        AttachDBFilename = ""
                    }.ConnectionString;

                    using (var masterconn = new SqlConnection(mastercs))
                    {
                        masterconn.Open();
                        var cmd = masterconn.CreateCommand();
                        cmd.CommandText = $"ALTER DATABASE [{db.Replace("]", "]]")}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = "EXEC sp_detach_db @dbname";
                        cmd.Parameters.AddWithValue("@dbname", db);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
    }
}