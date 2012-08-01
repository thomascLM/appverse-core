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
package com.gft.unity.core.system.server.net.handler;

import com.gft.unity.core.system.server.net.AbstractHandler;
import com.gft.unity.core.system.server.net.Handler;
import com.gft.unity.core.system.server.net.HttpRequest;
import com.gft.unity.core.system.server.net.HttpResponse;
import com.gft.unity.core.system.server.net.InternetOutputStream;
import com.gft.unity.core.system.server.net.Request;
import com.gft.unity.core.system.server.net.Response;
import com.gft.unity.core.system.server.net.Server;
import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.PrintWriter;
import java.util.Properties;

/**
 * <p> This Handler prints out the incoming Http Request, and echos it back as
 * plain text. This is great for debugging request being submitted to the
 * server. Past though this it has little production use. It does not filter by
 * URL so if it is called it short circuits any other handler down stream. If it
 * is installed at the head of a chain no other handlers in the chain will be
 * called. </p>
 */
public class PrintHandler extends AbstractHandler implements Handler {

    private Properties properties;
    private boolean showProperties = true;

    @Override
    public boolean initialize(String handlerName, Server server) {
        super.initialize(handlerName, server);

        properties = server.getConfig();

        return true;
    }

    @Override
    public boolean handle(Request aRequest, Response aResponse)
            throws IOException {

        if (aRequest instanceof HttpRequest) {

            HttpRequest request = (HttpRequest) aRequest;
            HttpResponse response = (HttpResponse) aResponse;

            StringBuilder buffer = new StringBuilder();
            if (showProperties) {
                buffer.append("Properties:\r\n");
                buffer.append(properties.toString());
                buffer.append("-EOF-\r\n");
            }
            buffer.append(request.toString());
            buffer.append("\r\n");
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            request.getHeaders().print(new InternetOutputStream(baos));
            if (request.getPostData() != null) {
                baos.write(request.getPostData());
            }
            buffer.append(baos.toString("UTF-8"));

            response.setMimeType("text/plain");
            PrintWriter out = response.getPrintWriter();
            out.write(buffer.toString());

            return true;
        }

        return false;
    }
}
