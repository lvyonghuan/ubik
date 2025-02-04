# API

## Add Node

To add a node to graph, logically.

```
POST /node
```

### Parameters
| Name | Position | Description |
|---|---------|---|
|name| Query   |Name of the node|

### Response

If successful, the response will be like:

```json
{
  "status": 200,
  "error": null,
  "data": {
    "id": 1
  }
}
```

## Remove Node

To remove a node from graph, logically.

```
DELETE /node
```

### Parameters
| Name | Position | Description |
|---|---|---|
|id|Query|ID of the node|

### Response

If successful, the response will be like:

```json
{
  "status": 200,
  "error": null,
  "data": null
}
```

## Add Edge

To add an edge to graph, logically.

```
POST /edge
```

### Parameters

| Name                | Position | Description                |
|---------------------|----------|----------------------------|
| producer_id         | body     | ID of the producer node    |
| consumer_id         | body     | ID of the consumer node    |
| producer_point_name | body     | Name of the producer point |
| consumer_point_name | body     | Name of the consumer point |

### Response

If successful, the response will be like:

```json
{
  "status": 200,
  "error": null,
  "data": null
}
```

## Delete Edge

```
DELETE /edge
```

### Parameters

| Name                | Position | Description                |
|---------------------|----------|----------------------------|
| producer_id         | body     | ID of the producer node    |
| consumer_id         | body     | ID of the consumer node    |
| producer_point_name | body     | Name of the producer point |
| consumer_point_name | body     | Name of the consumer point |

### Response

If successful, the response will be like:

```json
{
  "status": 200,
  "error": null,
  "data": null
}
```

## Before running set

Physically mount the plugin, and link nodes.

```
GET /run/set
```

### Parameters

This endpoint does not require any parameters. 

### Response

If successful, the response will be like:

```json
{
  "status": 200,
  "error": null,
  "data": null
}
```

## Run

Run the graph.

```
GET /run
```

### Parameters

This endpoint does not require any parameters. 

### Response

If successful, the response will be like:

```json
{
  "status": 200,
  "error": null,
  "data": null
}
```
