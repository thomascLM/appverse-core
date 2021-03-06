/*
 Copyright (c) 2012 GFT Appverse, S.L., Sociedad Unipersonal.

 This Source  Code Form  is subject to the  terms of  the Appverse Public License 
 Version 2.0  (“APL v2.0”).  If a copy of  the APL  was not  distributed with this 
 file, You can obtain one at http://www.appverse.mobi/licenses/apl_v2.0.pdf.

 Redistribution and use in  source and binary forms, with or without modification, 
 are permitted provided that the  conditions  of the  AppVerse Public License v2.0 
 are met.

 THIS SOFTWARE IS PROVIDED BY THE  COPYRIGHT HOLDERS  AND CONTRIBUTORS "AS IS" AND
 ANY EXPRESS  OR IMPLIED WARRANTIES, INCLUDING, BUT  NOT LIMITED TO,   THE IMPLIED
 WARRANTIES   OF  MERCHANTABILITY   AND   FITNESS   FOR A PARTICULAR  PURPOSE  ARE
 DISCLAIMED. EXCEPT IN CASE OF WILLFUL MISCONDUCT OR GROSS NEGLIGENCE, IN NO EVENT
 SHALL THE  COPYRIGHT OWNER  OR  CONTRIBUTORS  BE LIABLE FOR ANY DIRECT, INDIRECT,
 INCIDENTAL,  SPECIAL,   EXEMPLARY,  OR CONSEQUENTIAL DAMAGES  (INCLUDING, BUT NOT
 LIMITED TO,  PROCUREMENT OF SUBSTITUTE  GOODS OR SERVICES;  LOSS OF USE, DATA, OR
 PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT(INCLUDING NEGLIGENCE OR OTHERWISE) 
 ARISING  IN  ANY WAY OUT  OF THE USE  OF THIS  SOFTWARE,  EVEN  IF ADVISED OF THE 
 POSSIBILITY OF SUCH DAMAGE.
 */
package com.gft.unity.core.pim;

public interface IContacts {

    /**
     * Creates a Contact based on given contact data.
     *
     * @param contactData Contact data.
     * @return Created contact.
     */
    public Contact CreateContact(Contact contactData);

    /**
     * Deletes the given contact.
     *
     * @param contact Contact to be deleted.
     * @return <CODE>true</CODE> on successful deletion, <CODE>false</CODE>
     * otherwise.
     */
    public boolean DeleteContact(Contact contact);

    /**
     * List of stored phone contacts.
     *
     * @return List of contacts.
     */
    public Contact[] ListContacts();

    /**
     * List of stored phone contacts that match given query.
     *
     * @param queryText Search query.
     * @return List of contacts.
     */
    public Contact[] ListContacts(String queryText);

    /**
     * Updates contact data (given its ID) with the given contact data.
     *
     * @param ID Contact identifier.
     * @param newContactData Data to add to contact.
     * @return <CODE>true</CODE> on successful update, <CODE>false</CODE>
     * otherwise.
     */
    public boolean UpdateContact(String ID, Contact newContactData);
}
