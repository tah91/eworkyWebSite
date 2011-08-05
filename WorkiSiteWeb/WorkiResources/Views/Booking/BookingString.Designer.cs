﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Worki.Resources.Views.Booking {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class BookingString {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal BookingString() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Worki.Resources.Views.Booking.BookingString", typeof(BookingString).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Retour.
        /// </summary>
        public static string BackToUrl {
            get {
                return ResourceManager.GetString("BackToUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Détails de la réservation.
        /// </summary>
        public static string BookingDetails {
            get {
                return ResourceManager.GetString("BookingDetails", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remplir les champs pour réserver, un email vous sera envoyé.
        /// </summary>
        public static string BookingIntro {
            get {
                return ResourceManager.GetString("BookingIntro", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nom : {0}
        ///Téléphone : {1}
        ///E-mail : {2}
        ///Besoin : {3}
        ///Début : {4}
        ///Fin : {5}
        ///Message : {6}.
        /// </summary>
        public static string BookingMailBody {
            get {
                return ResourceManager.GetString("BookingMailBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alerte eWorky – Demande de réservation.
        /// </summary>
        public static string BookingMailSubject {
            get {
                return ResourceManager.GetString("BookingMailSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Réserver.
        /// </summary>
        public static string BookIt {
            get {
                return ResourceManager.GetString("BookIt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirmer.
        /// </summary>
        public static string ConfirmIt {
            get {
                return ResourceManager.GetString("ConfirmIt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bonjour,
        ///
        ///Votre réservation pour un(e) {0} du {1} à {2} dans l’établissement {3} ({4}) est confirmée. 
        ///Vous devrez régler la somme de {5} € directement auprès du prestataire.
        ///En espérant avoir satisfait votre demande,
        ///
        ///L’équipe eWorky.
        /// </summary>
        public static string ConfirmMailBody {
            get {
                return ResourceManager.GetString("ConfirmMailBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to eWorky – Votre réservation est confirmée.
        /// </summary>
        public static string ConfirmMailSubject {
            get {
                return ResourceManager.GetString("ConfirmMailSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Prendre en charge.
        /// </summary>
        public static string HandleIt {
            get {
                return ResourceManager.GetString("HandleIt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Bonjour,
        ///
        ///Votre demande de réservation pour un(e) {0} du {1} à {2} dans l’établissement {3} ({4}) a été prise en charge par nos équipes. 
        ///Notre équipe vous recontactera sous 48 H pour vous confirmer votre réservation.
        ///En espérant vous revoir très vite sur www.eworky.com,
        ///
        ///L’équipe eWorky.
        /// </summary>
        public static string HandleMailBody {
            get {
                return ResourceManager.GetString("HandleMailBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to eWorky – Votre demande de réservation a été prise en compte.
        /// </summary>
        public static string HandleMailSubject {
            get {
                return ResourceManager.GetString("HandleMailSubject", resourceCulture);
            }
        }
    }
}
