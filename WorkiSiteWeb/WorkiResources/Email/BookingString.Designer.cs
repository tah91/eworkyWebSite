﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.530
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
        ///   Looks up a localized string similar to Contacter le lieu suivant car une demande de réservation a été faite, alors que le lieu ne propose pas encore de réservation :
        ///Nom du lieu : &lt;b&gt;{0}&lt;/b&gt;.
        /// </summary>
        public static string AlertBookingNeed {
            get {
                return ResourceManager.GetString("AlertBookingNeed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Demande de possibilité de réservation.
        /// </summary>
        public static string AlertBookingNeedSubject {
            get {
                return ResourceManager.GetString("AlertBookingNeedSubject", resourceCulture);
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
        public static string BookingNewMember {
            get {
                return ResourceManager.GetString("BookingNewMember", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Un compte a été créé suite à votre demande de réservation.
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
        ///   Looks up a localized string similar to Le gérant de &lt;b&gt;{0}&lt;/b&gt; a ajouté votre réservation pour un(e) &lt;b&gt;{1}&lt;/b&gt; du &lt;b&gt;{2}&lt;/b&gt; au &lt;b&gt;{3}&lt;/b&gt; sur son agenda eWorky. 
        ///
        ///Vous pouvez dès à présent consulter cette réservation dans votre espace utilisateur &lt;b&gt;{4}&lt;/b&gt;. 
        ///
        ///Le montant de cette réservation est de &lt;b&gt;{5} €&lt;/b&gt;, nous vous invitons également à effectuer ce règlement en ligne via Paypal.
        ///Pour votre prochaine réservation, n’hésitez pas à passer directement par eworky.com pour plus de simplicité !
        ///
        ///Si cet espace vous a plu n’hésitez pas à l [rest of string was truncated]&quot;;.
        /// </summary>
        public static string CalandarBookingCreation {
            get {
                return ResourceManager.GetString("CalandarBookingCreation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre réservation chez {0} a été enregistrée sur eWorky.
        /// </summary>
        public static string CalandarBookingCreationSubject {
            get {
                return ResourceManager.GetString("CalandarBookingCreationSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Le gérant de &lt;b&gt;{0}&lt;/b&gt; a modifié votre réservation sur son agenda eWorky !
        ///Votre réservation est désormais enregistrée du &lt;b&gt;{1}&lt;/b&gt; au &lt;b&gt;{2}&lt;/b&gt;.
        ///
        ///Vous pouvez consulter vos différentes réservations dans votre espace utilisateur &lt;b&gt;{3}&lt;/b&gt;.
        ///Pour votre prochaine réservation, n’hésitez pas à passer directement par eworky.com pour plus de simplicité !
        ///
        ///Si cet espace vous a plu n’hésitez pas à le noter et commenter sur sa fiche eWorky &lt;b&gt;{4}&lt;/b&gt;..
        /// </summary>
        public static string CalandarBookingModification {
            get {
                return ResourceManager.GetString("CalandarBookingModification", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre réservation chez {0} a été modifiée.
        /// </summary>
        public static string CalandarBookingModificationSubject {
            get {
                return ResourceManager.GetString("CalandarBookingModificationSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Un gérant d&apos;espace vous a rajouté dans son fichier client, et un compte eWorky a été créé. Il vous permettra de suivre  l’historique et l’état de vos réservations.
        ///
        ///Votre login est &quot;&lt;b&gt;{0}&lt;/b&gt;&quot; et votre mot de passe temporaire est &lt;b&gt;&quot;{1}&quot;&lt;/b&gt;. Vous pouvez changer votre mot de passe allant sur votre profil : &lt;b&gt;{2}&lt;/b&gt;
        ///
        ///Nous vous invitons de plus à compléter votre profil en vous rendant à cette adresse : &lt;b&gt;{3}&lt;/b&gt;.
        /// </summary>
        public static string ClientCreationAccount {
            get {
                return ResourceManager.GetString("ClientCreationAccount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Un compte a été crée pour gérer vos réservations.
        /// </summary>
        public static string ClientCreationAccountSubject {
            get {
                return ResourceManager.GetString("ClientCreationAccountSubject", resourceCulture);
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
        ///   Looks up a localized string similar to Vous avez reçu une demande de devis pour un(e) {0} dans l&apos;établissement {1} ({2}). 
        ///
        ///Grace à votre {3}, vous rendez gratuitement votre lieu plus visible, et bénéficiez d’outils de gestion simples et indispensables (Réservation, paiement sécurisé, planning, factures automatisées, CRM).
        ///
        ///Rendez-vous dans votre {4} pour confirmer/refuser cette demande de réservation dans les meilleurs délais..
        /// </summary>
        public static string CreateQuotationAndBOOwner {
            get {
                return ResourceManager.GetString("CreateQuotationAndBOOwner", resourceCulture);
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
        ///   Looks up a localized string similar to Suite à votre inscription au Networking des coworkers, nous avons le plaisir de vous annoncer qu’un compte eWorky vous a été créé !
        ///
        ///Votre login est &quot;&lt;b&gt;{0}&lt;/b&gt;&quot; et votre mot de passe temporaire est &lt;b&gt;&quot;{1}&quot;&lt;/b&gt;. Vous pouvez changer votre mot de passe allant sur votre profil : &lt;b&gt;{2}&lt;/b&gt;
        ///
        ///Nous vous invitons de plus à compléter votre profil en vous rendant à cette adresse : &lt;b&gt;{3}&lt;/b&gt;.
        /// </summary>
        public static string PartyCreateAccount {
            get {
                return ResourceManager.GetString("PartyCreateAccount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre compte eWorky a été créé !.
        /// </summary>
        public static string PartyCreateAccountSubject {
            get {
                return ResourceManager.GetString("PartyCreateAccountSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre inscription à la soirée Networking coworkers a bien été enregistrée !
        ///Nous vous attendons dans les locaux de &lt;b&gt;{0}&lt;/b&gt; (&lt;b&gt;{1}&lt;/b&gt;) pour rencontrer autour d’un verre les coworkers et les espace de coworking parisiens. Vous aurez également la possibilité de faire un pitch de présentation de 45 secondes.
        ///N’hésitez pas à noter et commenter vos espaces préférés sur eWorky..
        /// </summary>
        public static string PartyRegister {
            get {
                return ResourceManager.GetString("PartyRegister", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre inscription au networking des coworkers est bien enregistrée !.
        /// </summary>
        public static string PartyRegisterSubject {
            get {
                return ResourceManager.GetString("PartyRegisterSubject", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Nom : &lt;b&gt;{0}&lt;/b&gt;
        ///Téléphone : &lt;b&gt;{1}&lt;/b&gt;
        ///Adresse électronique : &lt;b&gt;{2}&lt;/b&gt;
        ///Nom du lieu : &lt;b&gt;{3}&lt;/b&gt;.
        /// </summary>
        public static string PartyRegisterTeam {
            get {
                return ResourceManager.GetString("PartyRegisterTeam", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Alerte eWorky – Inscription networking.
        /// </summary>
        public static string PartyRegisterTeamSubject {
            get {
                return ResourceManager.GetString("PartyRegisterTeamSubject", resourceCulture);
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
        ///   Looks up a localized string similar to Malheureusement, votre demande de réservation pour un(e) {0} du {1} au {2}  dans l’établissement {3} ({4}) a été refusée, faute de disponibilité.
        ///
        ///Le gérant vous invite à réitérer votre demande à un autre moment ou pour une autre offre..
        /// </summary>
        public static string RefuseBookingClient {
            get {
                return ResourceManager.GetString("RefuseBookingClient", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Votre demande de réservation a été refusée par le gérant.
        /// </summary>
        public static string RefuseBookingClientSubject {
            get {
                return ResourceManager.GetString("RefuseBookingClientSubject", resourceCulture);
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
