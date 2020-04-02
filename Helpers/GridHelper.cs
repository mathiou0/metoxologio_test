using Kendo.Mvc;
using Kendo.Mvc.UI.Fluent;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace metoxologio_test.Helpers
{
    public static class GridHelper
    {
       
        static IAuthorizationService AuthorizationService { get; set; }
        public static GridBuilder<T> TestGridAsync<T>(this GridBuilder<T> helper, string name, bool flag)
            where T : class
        {

            return helper
                .Name(name)
                .Columns(

                 collumns => {
                     if (flag) {
                         collumns.Command(
                         commands =>
                         {
                             commands.Edit();
                         }).Title("Commands").Width(200);
                        }

                     })
                .ToolBar(toolbar =>
                    {
                        if (flag)
                            toolbar.Create();
                    }
                )
                .Groupable()
                .Pageable()
                .Sortable()
                .Scrollable()
                .Filterable()
                .Pageable();
        }
        
    }
}
