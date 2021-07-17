<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/">
<html> 
<body>
  <h2>My Books:</h2>
  <table border="1">
    <tr bgcolor="#9acd32">
    <th style="text-align:center">Title</th>
      <th style="text-align:center">Author</th>
      <th style="text-align:center">Category</th>
      <th style="text-align:center">Price</th>
      <th style="text-align:center">Year</th>      
    </tr>
    <xsl:for-each select="bookstore/book">
    <tr>
    <td><xsl:value-of select="title"/></td>
    <td>
    <xsl:for-each select="author">
    <xsl:value-of select="."/>
     <xsl:text disable-output-escaping="yes">;&lt;br/&gt;</xsl:text>
    </xsl:for-each>
    </td>
    <td><xsl:value-of select="@category"/></td>
    <td><xsl:value-of select="price"/></td>   
    <td><xsl:value-of select="year"/></td>    
    </tr>
    </xsl:for-each>
  </table>
</body>
</html>
</xsl:template>
</xsl:stylesheet>
