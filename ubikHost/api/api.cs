using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using ubique.util;

namespace ubikHost.api;

public class Router(Core core)
{
    private readonly Core _core=core;
    
    public void Init()
    {
        var builder = WebApplication.CreateBuilder();
        var app = builder.Build();

        app.MapPost("/node",(HttpContext h)=>AddNode(h));//添加节点
        app.MapDelete("/node", (HttpContext h)=>RemoveNode(h));//删除节点
        
        app.MapPost("/edge",(HttpContext h)=>AddEdge(h));//添加边
        app.MapDelete("/edge", (HttpContext h)=>RemoveEdge(h));//删除边

        var run=app.MapGroup("/run");
        run.MapGet("/set", (HttpContext h)=>BeforeRunSet(h));//运行前设置
        run.MapGet("/",(HttpContext h)=>Run(h));//运行

        app.Run();
    }
    
    private IResult AddNode(HttpContext context)
    {
        var nodeName = context.Request.Query["name"];
        if (string.IsNullOrEmpty(nodeName))
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = "nodeName is empty"
                });
        }

        int id;
        try{
            id = _core.AddNode(nodeName);
        }
        catch (UbikException e)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        
        return Results.Ok(
            new Response
            {
                Code = 200,
                Data = new{id =id}
            });
    }
    
    private IResult RemoveNode(HttpContext context)
    {
        var nodeId = context.Request.Query["id"];
        if (string.IsNullOrEmpty(nodeId))
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = "nodeId is empty"
                });
        }

        try{
            _core.RemoveNode(int.Parse(nodeId));
        }
        catch (UbikException e)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        
        return Results.Ok(
            new Response
            {
                Code = 200
            });
    }
    
    private IResult AddEdge(HttpContext context)
    {
        var producerNodeId = context.Request.Query["producer_id"];
        var consumerNodeId = context.Request.Query["consumer_id"];
        var producerPointName = context.Request.Query["producer_point_name"];
        var consumerPointName = context.Request.Query["consumer_point_name"];
        
        if (string.IsNullOrEmpty(producerNodeId) || string.IsNullOrEmpty(consumerNodeId) || string.IsNullOrEmpty(producerPointName) || string.IsNullOrEmpty(consumerPointName))
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = "parameter is empty"
                });
        }

        try{
            _core.UpdateEdge(int.Parse(producerNodeId),int.Parse(consumerNodeId),producerPointName,consumerPointName);
        }
        catch (UbikException e)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        
        return Results.Ok(
            new Response
            {
                Code = 200
            });
    }
    
    private IResult RemoveEdge(HttpContext context)
    {
        var producerNodeId = context.Request.Query["producer_id"];
        var consumerNodeId = context.Request.Query["consumer_id"];
        var producerPointName = context.Request.Query["producer_point_name"];
        var consumerPointName = context.Request.Query["consumer_point_name"];
        
        if (string.IsNullOrEmpty(producerNodeId) || string.IsNullOrEmpty(consumerNodeId) || string.IsNullOrEmpty(producerPointName) || string.IsNullOrEmpty(consumerPointName))
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = "parameter is empty"
                });
        }

        try{
            _core.DeleteEdge(int.Parse(producerNodeId),int.Parse(consumerNodeId),producerPointName,consumerPointName);
        }
        catch (UbikException e)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        
        return Results.Ok(
            new Response
            {
                Code = 200
            });
    }
    
    private IResult BeforeRunSet(HttpContext context)
    {
        var e=_core.BeforeRunSet();
        if (e!=null)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        
        return Results.Ok(
            new Response
            {
                Code = 200
            });
    }
    
    private IResult Run(HttpContext context)
    {
        try{
            Task.Run(() => _core.Run());
        }
        catch (UbikException e)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        catch (Exception e)
        {
            return Results.Ok(
                new Response
                {
                    Code = 400,
                    Error = e.Message
                });
        }
        
        return Results.Ok(
            new Response
            {
                Code = 200
            });
    }
    
 
    private class Response
    {
        public int Code;
        public string? Error;
        public object? Data;
    }
}