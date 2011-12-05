﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.488
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Worki.Resources.Email {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Worki.Resources.Email.BookingString", typeof(BookingString).Assembly);
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
        ///   Looks up a localized string similar to Votre demande de réservation pour un(e) &lt;b&gt;{0}&lt;/b&gt; du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt; dans l’établissement &lt;b&gt;{3}&lt;/b&gt; (&lt;b&gt;{4}&lt;/b&gt;) a été accepté pour la somme de &lt;b&gt;{5}&lt;/b&gt; €.
        ///
        ///Pour confirmer définitivement votre réservation, rendez-vous dans votre {6} pour régler votre réservation..
        /// </summary>
        public static string AcceptBookingClient {
            get {
                return ResourceManager.GetString("AcceptBookingClient", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de réservation a été validée par le gérant.
        /// </summary>
        public static string AcceptBookingClientSubject {
            get {
                return ResourceManager.GetString("AcceptBookingClientSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vous avez accepté la réservation pour un(e) &lt;b&gt;{0}&lt;/b&gt; du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt; dans l’établissement &lt;b&gt;{3}&lt;/b&gt; (&lt;b&gt;{4}&lt;/b&gt;) pour la somme de &lt;b&gt;{5}&lt;/b&gt;.
        /// </summary>
        public static string AcceptBookingOwner {
            get {
                return ResourceManager.GetString("AcceptBookingOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nom : &lt;b&gt;{0}&lt;/b&gt;
        ///Téléphone : &lt;b&gt;{1}&lt;/b&gt;
        ///Adresse électronique : &lt;b&gt;{2}&lt;/b&gt;
        ///Nom du lieu : &lt;b&gt;{3}&lt;/b&gt;
        ///Besoin : &lt;b&gt;{4}&lt;/b&gt;
        ///Début : &lt;b&gt;{5}&lt;/b&gt;
        ///Fin : &lt;b&gt;{6}&lt;/b&gt;
        ///Message : &lt;b&gt;{7}&lt;/b&gt;.
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
        ///   Looks up a localized string similar to Suite à votre demande de réservation pour un(e) &lt;b&gt;{0}&lt;/b&gt; du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt; dans l’établissement &lt;b&gt;{3}&lt;/b&gt; (&lt;b&gt;{4}&lt;/b&gt;), un compte eWorky a été créé. Il vous permettra de suivre  l’historique et l’état de vos réservations.
        ///
        ///Votre login est &quot;&lt;b&gt;{5}&lt;/b&gt;&quot; et votre mot de passe temporaire est &lt;b&gt;&quot;{6}&quot;&lt;/b&gt;. Vous pouvez changer votre mot de passe allant sur votre profil : &lt;b&gt;{7}&lt;/b&gt;
        ///
        ///Nous vous invitons de plus à compléter votre profil en vous rendant à cette adresse : &lt;b&gt;{8}&lt;/b&gt;.
        /// </summary>
        public static string BookingNewMemberBody {
            get {
                return ResourceManager.GetString("BookingNewMemberBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to eWorky – Un compte a été créé suite à votre demande de réservation.
        /// </summary>
        public static string BookingNewMemberSubject {
            get {
                return ResourceManager.GetString("BookingNewMemberSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vous avez reçu une demande de réservation pour un(e) {0} dans l&apos;établissement {1} ({2}). 
        ///
        ///Rendez-vous dans votre {3} pour confirmer/refuser cette demande de réservation dans les meilleurs délais..
        /// </summary>
        public static string BookingOwnerBody {
            get {
                return ResourceManager.GetString("BookingOwnerBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Demande de réservation - {0}.
        /// </summary>
        public static string BookingOwnerSubject {
            get {
                return ResourceManager.GetString("BookingOwnerSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de réservation pour un(e) &lt;b&gt;{0}&lt;/b&gt; du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt; dans l’établissement &lt;b&gt;{3}&lt;/b&gt; (&lt;b&gt;{4}&lt;/b&gt;) a été prise en compte. 
        ///Le gérant de l&apos;espace confirmera votre réservation dès que possible..
        /// </summary>
        public static string CreateBookingClient {
            get {
                return ResourceManager.GetString("CreateBookingClient", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de réservation a bien été prise en compte.
        /// </summary>
        public static string CreateBookingClientSubject {
            get {
                return ResourceManager.GetString("CreateBookingClientSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;b&gt;Création&lt;/b&gt;
        ///
        ///Nom : &lt;b&gt;{0}&lt;/b&gt;
        ///Téléphone : &lt;b&gt;{1}&lt;/b&gt;
        ///Adresse électronique : &lt;b&gt;{2}&lt;/b&gt;
        ///Nom du lieu : &lt;b&gt;{3}&lt;/b&gt;
        ///Besoin : &lt;b&gt;{4}&lt;/b&gt;
        ///Début : &lt;b&gt;{5}&lt;/b&gt;
        ///Fin : &lt;b&gt;{6}&lt;/b&gt;
        ///Message : &lt;b&gt;{7}&lt;/b&gt;.
        /// </summary>
        public static string CreateBookingTeam {
            get {
                return ResourceManager.GetString("CreateBookingTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de devis pour un(e) &lt;b&gt;{0}&lt;/b&gt; de &lt;b&gt;{1}&lt;/b&gt; m² dans l’établissement &lt;b&gt;{2}&lt;/b&gt; (&lt;b&gt;{3}&lt;/b&gt;) a été prise en compte. 
        ///Le gérant de l&apos;espace confirmera votre demande de devis dès que possible..
        /// </summary>
        public static string CreateQuotationClient {
            get {
                return ResourceManager.GetString("CreateQuotationClient", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de devis a bien été prise en compte.
        /// </summary>
        public static string CreateQuotationClientSubject {
            get {
                return ResourceManager.GetString("CreateQuotationClientSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vous avez reçu une demande de devis pour un(e) {0} dans l&apos;établissement {1} ({2}). 
        ///
        ///Rendez-vous dans votre {3} pour confirmer/refuser cette demande de réservation dans les meilleurs délais..
        /// </summary>
        public static string CreateQuotationOwner {
            get {
                return ResourceManager.GetString("CreateQuotationOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Demande de devis - {0}.
        /// </summary>
        public static string CreateQuotationOwnerSubject {
            get {
                return ResourceManager.GetString("CreateQuotationOwnerSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to La réservation n°{0} a été réglé par le client. Vous pouvez consulter les réservations confirmées dans votre espace gérant..
        /// </summary>
        public static string PayementOwner {
            get {
                return ResourceManager.GetString("PayementOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to La réservation n°{0} a été réglée.
        /// </summary>
        public static string PayementSubject {
            get {
                return ResourceManager.GetString("PayementSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nom : &lt;b&gt;{0}&lt;/b&gt;
        ///Téléphone : &lt;b&gt;{1}&lt;/b&gt;
        ///Adresse électronique : &lt;b&gt;{2}&lt;/b&gt;
        ///Nom du lieu : &lt;b&gt;{3}&lt;/b&gt;
        ///Besoin : &lt;b&gt;{4}&lt;/b&gt;
        ///Surface : &lt;b&gt;{5}&lt;/b&gt;
        ///Message : &lt;b&gt;{6}&lt;/b&gt;.
        /// </summary>
        public static string QuotationMailBody {
            get {
                return ResourceManager.GetString("QuotationMailBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alerte eWorky – Demande de devis.
        /// </summary>
        public static string QuotationMailSubject {
            get {
                return ResourceManager.GetString("QuotationMailSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Suite à votre demande de devis pour un(e) &lt;b&gt;{0}&lt;/b&gt; de &lt;b&gt;{1}&lt;/b&gt; m2 dans l’établissement &lt;b&gt;{2}&lt;/b&gt; (&lt;b&gt;{3}&lt;/b&gt;), un compte eWorky a été créé. Il vous, permettra de suivre  l’historique et l’état de vos demandes de devis et de vos réservations.
        ///
        ///Votre login est &lt;b&gt;{4}&lt;/b&gt; et votre mot de passe temporaire est &lt;b&gt;{5}&lt;/b&gt;. Vous pouvez changer votre mot de passe en allant sur votre profil : &lt;b&gt;{6}&lt;/b&gt;
        ///
        ///Nous vous invitons de plus à compléter votre profil en vous rendant à cette adresse : &lt;b&gt;{7}&lt;/b&gt;.
        /// </summary>
        public static string QuotationNewMemberBody {
            get {
                return ResourceManager.GetString("QuotationNewMemberBody", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to eWorky – Un compte a été créé suite à votre demande de devis.
        /// </summary>
        public static string QuotationNewMemberSubject {
            get {
                return ResourceManager.GetString("QuotationNewMemberSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de réservation pour un(e) &lt;b&gt;{0}&lt;/b&gt; du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt; dans l’établissement &lt;b&gt;{3}&lt;/b&gt; (&lt;b&gt;{4}&lt;/b&gt;) a été refusé pour la raison suivante : {5}.
        /// </summary>
        public static string RefuseBookingClient {
            get {
                return ResourceManager.GetString("RefuseBookingClient", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Vous avez refusé la réservation pour un(e) &lt;b&gt;{0}&lt;/b&gt; du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt; dans l’établissement &lt;b&gt;{3}&lt;/b&gt; (&lt;b&gt;{4}&lt;/b&gt;) pour la raison suivante : &lt;b&gt;{5}&lt;/b&gt;.
        /// </summary>
        public static string RefuseBookingOwner {
            get {
                return ResourceManager.GetString("RefuseBookingOwner", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de devis pour un(e) &lt;b&gt;{0}&lt;/b&gt; dans l’établissement &lt;b&gt;{1}&lt;/b&gt; (&lt;b&gt;{2}&lt;/b&gt;) a été refusé pour la raison suivante : {3}.
        /// </summary>
        public static string RefuseQuotationClient {
            get {
                return ResourceManager.GetString("RefuseQuotationClient", resourceCulture);
            }
        }
    }
}
