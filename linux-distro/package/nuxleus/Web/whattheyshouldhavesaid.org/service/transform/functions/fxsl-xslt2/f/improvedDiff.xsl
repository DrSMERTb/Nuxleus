<xsl:stylesheet version="2.0" 
xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
xmlns:f="http://fxsl.sf.net/"
xmlns:myImproveGenerator="myImproveGenerator"
xmlns:myImproveController="myImproveController" 
exclude-result-prefixes="f myImproveGenerator myImproveController"
>
  <xsl:import href="improve.xsl"/>
  <xsl:import href="takeWhile.xsl"/> 
  <xsl:import href="easyDiffList.xsl"/>

  <myImproveGenerator:myImproveGenerator/>
  <myImproveController:myImproveController/>
  
  <xsl:output indent="yes" omit-xml-declaration="yes"/>
  
  <xsl:template name="improvedDiff">
    <xsl:param name="pFun" select="/.."/>
    <xsl:param name="pX"/>
    <xsl:param name="pH0" select="0.1"/>
    <xsl:param name="pEpsRough" select="0.0001"/>
    <xsl:param name="pEpsImproved" select="0.0000001"/>

     <xsl:variable name="vMyImproveGenerator" select="document('')/*/myImproveGenerator:*[1]"/>
     <xsl:variable name="vMyImproveController" select="document('')/*/myImproveController:*[1]"/>
     
     <xsl:variable name="vrtfRoughResults">
	    <xsl:call-template name="easyDiffList">
		    <xsl:with-param name="pFun" select="$pFun"/>
		    <xsl:with-param name="pX" select="$pX"/>
		    <xsl:with-param name="pH0" select="$pH0"/>
		    <xsl:with-param name="pEps" select="$pEpsRough"/>
	    </xsl:call-template>
     </xsl:variable>
     
    <xsl:variable name="vrtfResults">
	    <xsl:call-template name="takeWhile">
	      <xsl:with-param name="pGenerator" select="$vMyImproveGenerator"/>
		  <xsl:with-param name="pParam0" select="$vrtfRoughResults/*"/>
		  <xsl:with-param name="pController" select="$vMyImproveController"/>
	      <xsl:with-param name="pContollerParam" select="$pEpsImproved"/>
		</xsl:call-template>
	</xsl:variable>
	
	<xsl:variable name="vResults" select="$vrtfResults/*"/>
	<xsl:value-of select="$vResults[last()]/*[2]"/>
	
  </xsl:template>
  
  <xsl:template name="myImproveGenerator" mode="f:FXSL"
       match="*[namespace-uri()='myImproveGenerator']">
     <xsl:param name="pList" select="/.."/>
     <xsl:param name="pParams" select="/.."/>
     
     <xsl:choose>
       <xsl:when test="not($pList)">
         <xsl:call-template name="improve">
	        <xsl:with-param name="pList" select="$pParams"/>
         </xsl:call-template>
       </xsl:when>
       <xsl:otherwise>
         <xsl:call-template name="improve">
	        <xsl:with-param name="pList" select="$pList[last()]/*"/>
         </xsl:call-template>
       </xsl:otherwise>
     </xsl:choose>

  </xsl:template>
  
  <xsl:template name="MyImproveController" mode="f:FXSL"
   match="*[namespace-uri()='myImproveController']">
    <xsl:param name="pList" select="/.."/>
    <xsl:param name="pParams" select="0.01"/>
    
    <xsl:choose>
      <xsl:when test="count($pList) &lt; 2">1</xsl:when>
      <xsl:otherwise>
        <xsl:variable name="vDiff" select="$pList[last()]/*[2] 
                                          - $pList[last() - 1]/*[2]"/>
        <xsl:if test="not($vDiff &lt; $pParams 
                     and $vDiff > (0 - $pParams))
                     and count($pList) &lt;= 5
                     ">1</xsl:if>
      </xsl:otherwise>
    </xsl:choose>
  
  </xsl:template>
</xsl:stylesheet>