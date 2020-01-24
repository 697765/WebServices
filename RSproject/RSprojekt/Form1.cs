using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using GMap.NET.MapProviders;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using RSprojekt.WebServisKarta;

namespace RSprojekt
{
    public partial class Form1 : Form
    {
        //stranica sa koje sam dobavila api https://ipstack.com/quickstart
      
        List<PointLatLng> lista_pozicija= new List<PointLatLng>();
        GMapOverlay mreza_oznaka= new GMapOverlay();
        GMapOverlay mreza_rute = new GMapOverlay();

        KartaSoapClient webServisKlijent = new KartaSoapClient();

        public Form1()
        {
            InitializeComponent();

            var lokacija_klijenta=webServisKlijent.DohvatiLokacijuKlijenta();
            var point_klijenta = new PointLatLng(lokacija_klijenta[0], lokacija_klijenta[1]);
            lista_pozicija.Add(point_klijenta);
            mreza_oznaka.Markers.Add(new GMarkerGoogle(point_klijenta, GMarkerGoogleType.arrow));
            IspisiOznake();

            Googl_map.MapProvider = GMapProviders.GoogleMap;
 
            Googl_map.Position = point_klijenta;

            Googl_map.MaxZoom = 100;
            Googl_map.MinZoom = 5;
            Googl_map.Zoom = 15;

            Googl_map.DragButton = MouseButtons.Left;
        }

        private void Googl_map_OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
        }

        private void Googl_map_OnMarkerDoubleClick(GMapMarker item, MouseEventArgs e)
        {
            if (lista_pozicija.Count != 1) { lista_pozicija.Remove(lista_pozicija[1]); richTextBox1.Text = ""; }

            Googl_map.Overlays.Remove(mreza_rute);
            lista_pozicija.Add(item.Position);

            GDirections upute;
            GoogleMapProvider.Instance.GetDirections(out upute, lista_pozicija[0], lista_pozicija[1], true, true, true, false, false);
            richTextBox1.Text = upute.ToString();

            var ruta = GoogleMapProvider.Instance.GetRoute(lista_pozicija[0], lista_pozicija[1], true, true, 5);

            if (mreza_rute.Routes.Count > 1) { mreza_rute.Clear(); }
            mreza_rute.Routes.Add(new GMapRoute(ruta.Points, "moja_ruta"));

            Googl_map.Overlays.Add(mreza_rute);
        }

        private void Googl_map_MouseDoubleClick(object sender, MouseEventArgs e)
        {
          
        }

        private void Googl_map_DoubleClick(object sender, EventArgs e)
        {
           /* PointLatLng point = new PointLatLng(lat, lon);
            GMapMarker oznaka = new GMarkerGoogle(point, GMarkerGoogleType.arrow);

            mreza_oznaka.Markers.Add(oznaka);

            IspisiOznake(); */  
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int radijus=0;

            if (string.IsNullOrEmpty(textBox1.Text)) { MessageBox.Show("Unesite radijus u textbox","",MessageBoxButtons.OK); }
            else {
                try { radijus=int.Parse(textBox1.Text); }
                catch { MessageBox.Show("radijus mora biti cjelobrojna vrijednost", "", MessageBoxButtons.OK); }
                }

            if (comboBox1.SelectedItem == null) { MessageBox.Show("Odaberite aktivnost iz padajućeg izbornika", "", MessageBoxButtons.OK); }

            var response=webServisKlijent.DohvatiAktivnosti(radijus,comboBox1.SelectedItem.ToString());
            var lista_lat_lng = webServisKlijent.DohvatiLokacijeAktivnosti(response);
            var lista_slika = webServisKlijent.DohvatiSlikeAktivnosti(response,comboBox1.SelectedItem.ToString());

            if (mreza_oznaka.Markers.Count != 1)
            {
                var lokacija_klijenta = mreza_oznaka.Markers[0];
                mreza_oznaka.Markers.Clear();
                mreza_oznaka.Markers.Add(lokacija_klijenta);

                Googl_map.Overlays.Clear();
            }

            for(int i=0;i<lista_slika.Length;i++)
            {
                var s = (System.Drawing.Bitmap)System.Drawing.Image.FromFile($@"C:/Users/korisnik/Downloads/RSprojekt/Slike/{i}.png");
                System.Drawing.Bitmap umanjeno = new System.Drawing.Bitmap(s,new Size(20,20));
                mreza_oznaka.Markers.Add(new GMarkerGoogle(new PointLatLng(lista_lat_lng[i * 2], lista_lat_lng[i * 2 + 1]), umanjeno));
                //mreza_oznaka.Markers.Add(new GMarkerGoogle(new PointLatLng(lista_lat_lng[i * 2], lista_lat_lng[i * 2 + 1]),GMarkerGoogleType.green_pushpin));     
            }

            IspisiOznake();
            
            richTextBox1.Text = "";
        }

        private void IspisiOznake()
        {
            Googl_map.Overlays.Add(mreza_oznaka);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
