<#-- @ftlvariable name="pm" type="de.monticore.trafo.templating.PatternMatchingAdapter"  -->
<#--We match a Feature as $F with a -->

<#--We could use 4 trafos and match the concrete syntax, but due to performance reasons we divide our matching time by 4-->
<#--We actually have to insert some abstract syntax by means of (Reference $FRef) here, as otherwise the references ID is matched-->
<#--(it also improves the performance by... a ton, as no ref->id chain has to be checked)-->
<#list pm.match("Reference $FRef <LB> <BS> alternative      <LB> <BS> Feature [[  Reference $IRef <LB> ]] <BE>      <BE>", ast) as match>
  ${match['$FRef'].get().asString()} <#t>
  -> <#t>
  ${match['$IRef'].get().asString()} <#t>
  [arrowhead="none", arrowtail="odot", dir="both"]<#t>


</#list>
