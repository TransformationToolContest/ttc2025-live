<#-- @ftlvariable name="pm" type="de.monticore.trafo.templating.PatternMatchingAdapter"  -->
<#--We match a Feature as $F :  Feature $F [[ concrete syntax of the Feature  ]]-->
<#--And the concrete syntax of the feature contains an optional {abstract}, matched as $A-->
<#--Reference $FName - its abstract syntax reduces the chain overhead (by avoiding the match onto the Id-->
<#--unfortunately UVL uses linebreaks and indentation  -->
<#list pm.match("Reference $FName opt [[ $A [[ {abstract} ]] ]]  <LB>  ", ast) as match>
  ${match['$FName'].get().asString()}
  [fillcolor="#ABACEA" tooltip="Cardinality: None" shape="${match['$A'].isPresent()?then("invhouse","box")}"]
</#list>
