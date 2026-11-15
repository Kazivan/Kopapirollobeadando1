using System;
using System.Linq;
using System.Windows;

namespace Autokolcsonzo
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            AlapDolgozokBetoltese();
        }

        private void AlapDolgozokBetoltese()
        {
            using (var cn = new cnAutokolcsonzo())
            {
                try
                {
                    cn.Database.EnsureCreated();
                    if (!cn.Dolgozok.Any())
                    {
                        cn.Dolgozok.AddRange(
                            new Dolgozo { Nev = "admin", Jelszo = "admin", Beosztas = "admin" },
                            new Dolgozo { Nev = "Ugyintezo1", Jelszo = "U1", Beosztas = "ugyintezo" },
                            new Dolgozo { Nev = "Szerelo1", Jelszo = "Sz1", Beosztas = "szerelo" }
                        );
                        cn.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Adatbázis kapcsolat hiba: " + ex.Message);
                }
            }
        }

        private void btnBelepes_Click(object sender, RoutedEventArgs e)
        {
            using (var cn = new cnAutokolcsonzo())
            {
                var dolgozo = cn.Dolgozok.FirstOrDefault(d => d.Nev == tbNev.Text && d.Jelszo == pbJelszo.Password);
                if (dolgozo != null)
                {
                    this.Hide();
                    var foablak = new MainWindow(dolgozo);
                    foablak.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Nem létező/nem jó felhasználó!", "Hiba", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }
}