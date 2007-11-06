<?xml version="1.0" encoding="UTF-8"?>
<xsl:transform version="2.0" 
xmlns:xs="http://www.w3.org/2001/XMLSchema" 
xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" 
xmlns:fn="http://www.w3.org/2005/xpath-functions" 
xmlns:saxon="http://saxon.sf.net/" 
xmlns:clitype="http://saxon.sf.net/clitype" 
xmlns:at="http://atomictalk.org" 
xmlns:func="http://atomictalk.org/function" 
xmlns:session="http://xameleon.org/service/session" 
xmlns:sguid="clitype:System.Guid?partialname=mscorlib" 
xmlns:aspnet-context="clitype:System.Web.HttpContext?partialname=System.Web" 
xmlns:proxy="http://xameleon.org/service/proxy" 
xmlns:html="http://www.w3.org/1999/xhtml" 
xmlns:operation="http://xameleon.org/service/operation" 
exclude-result-prefixes="xs xsl xsi fn clitype at func http-sgml-to-xml aspnet-context proxy saxon html">

  <xsl:import href="../../../../functions/funcset-Util.xslt" />
  <xsl:param name="current-context" />

  <xsl:template match="session:validate-request">
    <xsl:variable name="openid-session-id" select="func:resolve-variable(@key)" />
    <xsl:apply-templates>
      <xsl:with-param name="openid-session-id" select="$openid-session-id"/>
    </xsl:apply-templates>
  </xsl:template>

  <xsl:template match="operation:return-xml">
    <session>
      <xsl:apply-templates />
    </session>
  </xsl:template>

  <xsl:template match="session:generate-guid">
    <quid>
      <xsl:value-of select="string(sguid:NewGuid())"/>
    </quid>
  </xsl:template>

</xsl:transform>
