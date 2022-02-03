<?xml version="1.0" encoding="utf-8"?>
<!DOCTYPE html>
<html xsl:version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <head>
        <meta http-equiv="X-UA-Compatible" content="IE=11"/>
    </head>
    <style>
        * {
            -moz-box-sizing: border-box; 
            -webkit-box-sizing: border-box; 
            box-sizing: border-box;
        }
        body {
            font-family: Calibri, sans-serif;
            font-size: 12pt;
        }
        p,h1,h2,h3,h4 {
            margin: 0;
        }
        table {
            table-layout: fixed;
            border-collapse: collapse;
            border: 1px solid black;
        }
        table td,
        table th {
            padding: 0;
            margin: 0;
        }

        .mb-sm {
            margin-bottom: 10px;
        }
        .mb-lg {
            margin-bottom: 20px;
        }
        .mb-xlg {
            margin-bottom: 50px;
        }

        .bill_label_container {
            display: inline-block;
            vertical-align: top;
            margin: 15px 5px;
        }

        .bill_label .description {
            color: gray;
        }

        /* start label big */
        .bill_label_lg {
            width: 150mm;
            height: 30mm;
        }

        .bill_label_lg .category {
            text-align: center;
            vertical-align: middle;
            width: 3em;
        }

        .bill_label_lg .category p {
            -moz-transform: rotate(-90.0deg);  /* FF3.5+ */
            -o-transform: rotate(-90.0deg);  /* Opera 10.5 */
            -webkit-transform: rotate(-90.0deg);  /* Saf3.1+, Chrome */
            transform: rotate(-90.0deg);  /* Saf3.1+, Chrome */
            filter:  progid:DXImageTransform.Microsoft.BasicImage(rotation=0.083);  /* IE6,IE7 */
            -ms-filter: "progid:DXImageTransform.Microsoft.BasicImage(rotation=0.083)"; /* IE8 */
            margin-left: -1.3em;
            margin-right: -1.3em;
            font-size: 10pt;
        }

        .bill_label_lg .biller_name_description div {
            margin: 0 20px;
        }

        .bill_label_lg .biller_name p {
            font-weight: bold;
            font-size: 20pt;
        }
        /* end label big */

        /* start label small */
        .bill_label_sm {
            width: 110mm;
            height: 15mm;
        }

        .bill_label_sm .category {
            border: 1px solid black;
            width: 100px;
        }

        .bill_label_sm .category p {
            text-align: center;
            vertical-align: middle;
            font-size: 10pt;
            margin: 5px;
        }

        .bill_label_sm .biller_name_description div {
            margin: 0 10px;
        }

        .bill_label_sm .biller_name p {
            font-weight: bold;
        }
        /* end label small */
    </style>
    <body>
        <xsl:for-each select="ArrayOfBillXml/BillXml">
            <div class="container">
                <div class="bill_label_container">
                    <table class="bill_label bill_label_lg">
                        <colgroup>
                            <col style="border: 1px solid black;"/>
                            <col style="border: 1px solid black;"/>
                            <col/>
                        </colgroup>
                        <tr>
                            <td class="category">
                                <p><xsl:value-of select="CategoryName"/></p>
                            </td>
                            <td class="biller_name_description">
                                <div class="biller_name">
                                    <p><xsl:value-of select="BillerName"/></p>
                                </div>
                                <div class="description">
                                    <p><xsl:value-of select="Description"/></p>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
               
                <div class="bill_label_container">
                    <table class="bill_label bill_label_sm">
                        <tr>
                            <td class="category">
                                <p><xsl:value-of select="CategoryName"/></p>
                            </td>
                            <td class="biller_name_description">
                                <div class="biller_name">
                                    <p><xsl:value-of select="BillerName"/></p>
                                </div>
                                <div class="description">
                                    <p><xsl:value-of select="Description"/></p>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>  
            </div>
        </xsl:for-each>
    </body>
</html>