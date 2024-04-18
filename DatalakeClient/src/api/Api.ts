/* eslint-disable */
/* tslint:disable */
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

import {
  BlockInfo,
  BlockSimpleInfo,
  BlockTreeInfo,
  SourceEntryInfo,
  SourceInfo,
  TagInfo,
  ValuesGetValuesPayload,
  ValuesResponse,
  ValuesWriteValuesPayload,
} from "./data-contracts";
import { ContentType, HttpClient, RequestParams } from "./http-client";

export class Api<SecurityDataType = unknown> extends HttpClient<SecurityDataType> {
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksCreate
   * @request POST:/api/Blocks
   * @response `200` `number`
   */
  blocksCreate = (blockInfo: BlockInfo, params: RequestParams = {}) =>
    this.request<number, any>({
      path: `/api/Blocks`,
      method: "POST",
      body: blockInfo,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksReadAll
   * @request GET:/api/Blocks
   * @response `200` `(BlockSimpleInfo)[]`
   */
  blocksReadAll = (params: RequestParams = {}) =>
    this.request<BlockSimpleInfo[], any>({
      path: `/api/Blocks`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksRead
   * @request GET:/api/Blocks/{id}
   * @response `200` `BlockInfo`
   */
  blocksRead = (id: number, params: RequestParams = {}) =>
    this.request<BlockInfo, any>({
      path: `/api/Blocks/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksUpdate
   * @request PUT:/api/Blocks/{id}
   * @response `200` `File | null`
   */
  blocksUpdate = (id: number, block: BlockInfo, params: RequestParams = {}) =>
    this.request<File | null, any>({
      path: `/api/Blocks/${id}`,
      method: "PUT",
      body: block,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksDelete
   * @request DELETE:/api/Blocks/{id}
   * @response `200` `File | null`
   */
  blocksDelete = (id: number, params: RequestParams = {}) =>
    this.request<File | null, any>({
      path: `/api/Blocks/${id}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Blocks
   * @name BlocksReadAsTree
   * @request GET:/api/Blocks/tree
   * @response `200` `(BlockTreeInfo)[]`
   */
  blocksReadAsTree = (params: RequestParams = {}) =>
    this.request<BlockTreeInfo[], any>({
      path: `/api/Blocks/tree`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesCreate
   * @request POST:/api/Sources
   * @response `200` `number`
   */
  sourcesCreate = (source: SourceInfo, params: RequestParams = {}) =>
    this.request<number, any>({
      path: `/api/Sources`,
      method: "POST",
      body: source,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesReadAll
   * @request GET:/api/Sources
   * @response `200` `(SourceInfo)[]`
   */
  sourcesReadAll = (params: RequestParams = {}) =>
    this.request<SourceInfo[], any>({
      path: `/api/Sources`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesRead
   * @request GET:/api/Sources/{id}
   * @response `200` `SourceInfo`
   */
  sourcesRead = (id: number, params: RequestParams = {}) =>
    this.request<SourceInfo, any>({
      path: `/api/Sources/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesUpdate
   * @request PUT:/api/Sources/{id}
   * @response `200` `File | null`
   */
  sourcesUpdate = (id: number, source: SourceInfo, params: RequestParams = {}) =>
    this.request<File | null, any>({
      path: `/api/Sources/${id}`,
      method: "PUT",
      body: source,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesDelete
   * @request DELETE:/api/Sources/{id}
   * @response `200` `File | null`
   */
  sourcesDelete = (id: number, params: RequestParams = {}) =>
    this.request<File | null, any>({
      path: `/api/Sources/${id}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Sources
   * @name SourcesGetItemsWithTags
   * @request GET:/api/Sources/{id}/tags
   * @response `200` `(SourceEntryInfo)[]`
   */
  sourcesGetItemsWithTags = (id: number, params: RequestParams = {}) =>
    this.request<SourceEntryInfo[], any>({
      path: `/api/Sources/${id}/tags`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsCreate
   * @request POST:/api/Tags
   * @response `200` `number`
   */
  tagsCreate = (tag: TagInfo, params: RequestParams = {}) =>
    this.request<number, any>({
      path: `/api/Tags`,
      method: "POST",
      body: tag,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsReadAll
   * @request GET:/api/Tags
   * @response `200` `(TagInfo)[]`
   */
  tagsReadAll = (params: RequestParams = {}) =>
    this.request<TagInfo[], any>({
      path: `/api/Tags`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsRead
   * @request GET:/api/Tags/{id}
   * @response `200` `TagInfo`
   */
  tagsRead = (id: number, params: RequestParams = {}) =>
    this.request<TagInfo, any>({
      path: `/api/Tags/${id}`,
      method: "GET",
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsUpdate
   * @request PUT:/api/Tags/{id}
   * @response `200` `File | null`
   */
  tagsUpdate = (id: number, tag: TagInfo, params: RequestParams = {}) =>
    this.request<File | null, any>({
      path: `/api/Tags/${id}`,
      method: "PUT",
      body: tag,
      type: ContentType.Json,
      ...params,
    });
  /**
   * No description
   *
   * @tags Tags
   * @name TagsDelete
   * @request DELETE:/api/Tags/{id}
   * @response `200` `File | null`
   */
  tagsDelete = (id: number, params: RequestParams = {}) =>
    this.request<File | null, any>({
      path: `/api/Tags/${id}`,
      method: "DELETE",
      ...params,
    });
  /**
   * No description
   *
   * @tags Values
   * @name ValuesGetValues
   * @request POST:/api/Tags/Values
   * @response `200` `(ValuesResponse)[]`
   */
  valuesGetValues = (requests: ValuesGetValuesPayload, params: RequestParams = {}) =>
    this.request<ValuesResponse[], any>({
      path: `/api/Tags/Values`,
      method: "POST",
      body: requests,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
  /**
   * No description
   *
   * @tags Values
   * @name ValuesWriteValues
   * @request PUT:/api/Tags/Values
   * @response `200` `(ValuesResponse)[]`
   */
  valuesWriteValues = (requests: ValuesWriteValuesPayload, params: RequestParams = {}) =>
    this.request<ValuesResponse[], any>({
      path: `/api/Tags/Values`,
      method: "PUT",
      body: requests,
      type: ContentType.Json,
      format: "json",
      ...params,
    });
}
