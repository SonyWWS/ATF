<?xml version="1.0" encoding="utf-8"?>

<!-- from: https://issues.apache.org/bugzilla/show_bug.cgi?id=40886 -->

<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
    <xsl:output omit-xml-declaration="yes" indent="yes"/>
    <xsl:strip-space elements="*"/>
  
    <xsl:output method="xml" indent="yes" encoding="utf-8" version="1.0" cdata-section-elements="failure" />

    <xsl:template match="test-results/test-suite">

        <xsl:variable name="testcases" select="//test-suite[./results/test-case]"/>
        <xsl:variable name="asmClass">
                <xsl:choose>
                        <xsl:when test="$testcases"><xsl:value-of select="$testcases[1]/../../@name"/></xsl:when>
                        <xsl:otherwise><xsl:value-of select="@name"/></xsl:otherwise>
                </xsl:choose>
        </xsl:variable>
        <testsuite name="{$asmClass}" time="{@time}" tests="{count($testcases//test-case)}" 
            errors="" failures="{count($testcases//test-case/failure)}">

            <xsl:for-each select="$testcases">
                <xsl:variable name="suite" select="."/>
                <xsl:variable name="generalfailure" select="./failure"/>
                <xsl:for-each select=".//test-case">
                <testcase classname="{$asmClass}" name="{substring-after(./@name, concat($asmClass,'.'))}" time="{./@time}">
                <xsl:if test="./failure">
                <xsl:variable name="failstack" select="count(./failure/stack-trace/*) + count(./failure/stack-trace/text())"/>
                <failure>
                    <xsl:choose>
                        <xsl:when test="$failstack &gt; 0 or not($generalfailure)">
                            MESSAGE: <xsl:value-of select="./failure/message"/>
                            +++++++++++++++++++
                            STACK TRACE: <xsl:value-of select="./failure/stack-trace"/>
                        </xsl:when>
                        <xsl:otherwise>
                            MESSAGE: <xsl:value-of select="$generalfailure/message"/>
                            +++++++++++++++++++
                            STACK TRACE: <xsl:value-of select="$generalfailure/stack-trace"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </failure>
                </xsl:if>
                </testcase>
                </xsl:for-each>
            </xsl:for-each>

        </testsuite>

    </xsl:template>

</xsl:stylesheet> 
