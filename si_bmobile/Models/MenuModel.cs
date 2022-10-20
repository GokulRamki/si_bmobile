using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace si_bmobile.Models
{

    //public class tbl_main_menu
    //{
    //    [Key()]
    //    public int id { get; set; }

    //    [Required(ErrorMessage="Menu name is required")]
    //    public string menu_name { get; set; }

    //    public bool is_active { get; set; }

    //    public bool is_deleted { get; set; }

    //}

    //public class tbl_sub_menu
    //{
    //    [Key()]
    //    public int id { get; set; }
    //    [Required(ErrorMessage = "Sub menu name is required")]
    //    public string menu_name { get; set; }
    //    public bool is_active { get; set; }
    //    public bool is_deleted { get; set; }
    //}

    public class tbl_menu
    {
        [Key()]
        public int id { get; set; }

        [Required(ErrorMessage = "Menu name is required")]
        public string menu_name { get; set; }

        [Required(ErrorMessage = "Page name is required")]
        public string page_name { get; set; }

        public string parent_title { get; set; }

        public bool is_active { get; set; }

        public bool is_deleted { get; set; }
    }


    //public class tbl_main_sub_menu_conj
    //{
    //    [Key()]
    //    public int id { get; set; }
    //    public int main_menu_id { get; set; }
    //    public int sub_menu_id { get; set; }
    //}

    public class tbl_user_menu_conj
    {
        [Key()]
        public int id { get; set; }

        public long user_id { get; set; }

        public int menu_id { get; set; }
    }


    //customize

    //public class main_sub_menus
    //{
    //    public int main_menu_id { get; set; }

    //    public int sub_menu_id { get; set; }

    //    public string main_menu_name { get; set; }

    //    public string sub_menu_name { get; set; }

    //    public bool active { get; set; }
    //}


    public class Menu_Model
    {
        public int Menu_Id { get; set; }

        //public string menu_id { get; set; }

        public string menu_name { get; set; }

        public string parent_title { get; set; }

        public bool active { get; set; }
    }

    public class MainMenuModel
    {
        public string title { get; set; }

        public List<menumodel> menus { get; set; }
    }


    public class menumodel
    {
        public int menu_id { get; set; }

        public string menu_name { get; set; }

        public string page_name { get; set; }
    }





}