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

export enum AggregationFunc {
  List = "List",
  Sum = "Sum",
  Avg = "Avg",
  Min = "Min",
  Max = "Max",
}

export interface Entity {
  /** @format int32 */
  id?: number;
  /** @format int32 */
  parentId?: number;
  /** @format uuid */
  globalId?: string;
  name?: string | null;
  description?: string | null;
  children?: Entity[] | null;
  fields?: EntityField[] | null;
  relatedTags?: EntityTag[] | null;
  tags?: Tag[] | null;
}

export interface EntityField {
  /** @format int32 */
  id?: number;
  /** @format int32 */
  entityId?: number;
  name?: string | null;
  value?: string | null;
  type?: TagType;
  entity?: Entity;
}

export interface EntityTag {
  /** @format int32 */
  entityId?: number;
  /** @format int32 */
  tagId?: number;
  name?: string | null;
  entity?: Entity;
  tag?: Tag;
}

export interface HistoryRecord {
  /** @format date-time */
  date?: string;
  value?: any;
  quality?: TagQuality;
  using?: TagUsing;
}

export interface HistoryRequest {
  tags?: number[] | null;
  tagNames?: string[] | null;
  /** @format date-time */
  old?: string | null;
  /** @format date-time */
  young?: string | null;
  /** @format date-time */
  exact?: string | null;
  /** @format int32 */
  resolution?: number;
  func?: AggregationFunc;
}

export interface HistoryResponse {
  /** @format int32 */
  id?: number;
  tagName?: string | null;
  type?: TagType;
  func?: AggregationFunc;
  values?: HistoryRecord[] | null;
}

export interface LiveRequest {
  tags?: number[] | null;
  tagNames?: string[] | null;
}

export interface Source {
  /** @format int32 */
  id?: number;
  name?: string | null;
  type?: SourceType;
  address?: string | null;
  tags?: Tag[] | null;
}

export interface SourceRecord {
  path?: string | null;
  type?: TagType;
  relatedTag?: Tag;
}

export enum SourceType {
  Inopc = "Inopc",
  Datalake = "Datalake",
  Custom = "Custom",
}

export interface Tag {
  /** @format int32 */
  id?: number;
  /** @format uuid */
  globalId?: string;
  name?: string | null;
  description?: string | null;
  type?: TagType;
  /** @format date-time */
  created?: string;
  /** @format int32 */
  sourceId?: number;
  sourceItem?: string | null;
  isScaling?: boolean;
  /** @format float */
  minEU?: number;
  /** @format float */
  maxEU?: number;
  /** @format float */
  minRaw?: number;
  /** @format float */
  maxRaw?: number;
  source?: Source;
  inputs?: TagInput[] | null;
  relatedEntities?: EntityTag[] | null;
  entities?: Entity[] | null;
}

export interface TagHistory {
  /** @format int32 */
  tagId?: number;
  /** @format date-time */
  date?: string;
  text?: string | null;
  /** @format float */
  number?: number | null;
  quality?: TagQuality;
  using?: TagUsing;
}

export interface TagInput {
  /** @format int32 */
  resultTagId?: number;
  /** @format int32 */
  inputTagId?: number;
  variableName?: string | null;
  resultTag?: Tag;
  inputTag?: Tag;
}

export enum TagQuality {
  Bad = "Bad",
  BadNoConnect = "Bad_NoConnect",
  BadNoValues = "Bad_NoValues",
  BadManualWrite = "Bad_ManualWrite",
  Good = "Good",
  GoodManualWrite = "Good_ManualWrite",
  Unknown = "Unknown",
}

export enum TagType {
  String = "String",
  Number = "Number",
  Boolean = "Boolean",
}

export enum TagUsing {
  Initial = "Initial",
  Basic = "Basic",
  Aggregated = "Aggregated",
  Continuous = "Continuous",
  Outdated = "Outdated",
  NotFound = "NotFound",
}

export interface ValueRequest {
  /** @format int32 */
  tagId?: number | null;
  tagName?: string | null;
  value?: any;
  /** @format date-time */
  date?: string;
  tagQuality?: TagQuality;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<FullRequestParams, "body" | "method" | "query" | "path">;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (securityData: SecurityDataType | null) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown> extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) => fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter((key) => "undefined" !== typeof query[key]);
    return keys
      .map((key) => (Array.isArray(query[key]) ? this.addArrayQueryParam(query, key) : this.addQueryParam(query, key)))
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string") ? JSON.stringify(input) : input,
    [ContentType.Text]: (input: any) => (input !== null && typeof input !== "string" ? JSON.stringify(input) : input),
    [ContentType.FormData]: (input: any) =>
      Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
            ? JSON.stringify(property)
            : `${property}`,
        );
        return formData;
      }, new FormData()),
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(params1: RequestParams, params2?: RequestParams): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (cancelToken: CancelToken): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(`${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`, {
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type && type !== ContentType.FormData ? { "Content-Type": type } : {}),
      },
      signal: (cancelToken ? this.createAbortSignal(cancelToken) : requestParams.signal) || null,
      body: typeof body === "undefined" || body === null ? null : payloadFormatter(body),
    }).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const data = !responseFormat
        ? r
        : await response[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title DatalakeApp
 * @version 1.0
 */
export class Api<SecurityDataType extends unknown> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Entities
     * @name EntitiesCreate
     * @request POST:/api/Entities
     */
    entitiesCreate: (data: Entity, params: RequestParams = {}) =>
      this.request<Entity, any>({
        path: `/api/Entities`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Entities
     * @name EntitiesList
     * @request GET:/api/Entities
     */
    entitiesList: (params: RequestParams = {}) =>
      this.request<Entity[], any>({
        path: `/api/Entities`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Entities
     * @name EntitiesDetail
     * @request GET:/api/Entities/{id}
     */
    entitiesDetail: (id: number, params: RequestParams = {}) =>
      this.request<Entity, any>({
        path: `/api/Entities/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Entities
     * @name EntitiesUpdate
     * @request PUT:/api/Entities/{id}
     */
    entitiesUpdate: (id: number, data: Entity, params: RequestParams = {}) =>
      this.request<Entity, any>({
        path: `/api/Entities/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Entities
     * @name EntitiesDelete
     * @request DELETE:/api/Entities/{id}
     */
    entitiesDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Entities/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Entities
     * @name EntitiesTreeList
     * @request GET:/api/Entities/tree
     */
    entitiesTreeList: (params: RequestParams = {}) =>
      this.request<Entity[], any>({
        path: `/api/Entities/tree`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name SourcesCreate
     * @request POST:/api/Sources
     */
    sourcesCreate: (data: Source, params: RequestParams = {}) =>
      this.request<Source, any>({
        path: `/api/Sources`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name SourcesList
     * @request GET:/api/Sources
     */
    sourcesList: (params: RequestParams = {}) =>
      this.request<Source[], any>({
        path: `/api/Sources`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name SourcesDetail
     * @request GET:/api/Sources/{id}
     */
    sourcesDetail: (id: number, params: RequestParams = {}) =>
      this.request<Source, any>({
        path: `/api/Sources/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name SourcesUpdate
     * @request PUT:/api/Sources/{id}
     */
    sourcesUpdate: (id: number, data: Source, params: RequestParams = {}) =>
      this.request<Source, any>({
        path: `/api/Sources/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name SourcesDelete
     * @request DELETE:/api/Sources/{id}
     */
    sourcesDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Sources/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name SourcesTagsDetail
     * @request GET:/api/Sources/{id}/tags
     */
    sourcesTagsDetail: (id: number, params: RequestParams = {}) =>
      this.request<SourceRecord[], any>({
        path: `/api/Sources/${id}/tags`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tags
     * @name TagsCreate
     * @request POST:/api/Tags
     */
    tagsCreate: (data: Tag, params: RequestParams = {}) =>
      this.request<Tag, any>({
        path: `/api/Tags`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tags
     * @name TagsList
     * @request GET:/api/Tags
     */
    tagsList: (params: RequestParams = {}) =>
      this.request<Tag[], any>({
        path: `/api/Tags`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tags
     * @name TagsDetail
     * @request GET:/api/Tags/{id}
     */
    tagsDetail: (id: number, params: RequestParams = {}) =>
      this.request<Tag, any>({
        path: `/api/Tags/${id}`,
        method: "GET",
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tags
     * @name TagsUpdate
     * @request PUT:/api/Tags/{id}
     */
    tagsUpdate: (id: number, data: Tag, params: RequestParams = {}) =>
      this.request<Tag, any>({
        path: `/api/Tags/${id}`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Tags
     * @name TagsDelete
     * @request DELETE:/api/Tags/{id}
     */
    tagsDelete: (id: number, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/Tags/${id}`,
        method: "DELETE",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Values
     * @name TagsValuesLiveCreate
     * @request POST:/api/tags/Values/live
     */
    tagsValuesLiveCreate: (data: LiveRequest, params: RequestParams = {}) =>
      this.request<HistoryResponse[], any>({
        path: `/api/tags/Values/live`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Values
     * @name TagsValuesHistoryCreate
     * @request POST:/api/tags/Values/history
     */
    tagsValuesHistoryCreate: (data: HistoryRequest[], params: RequestParams = {}) =>
      this.request<HistoryResponse[], any>({
        path: `/api/tags/Values/history`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Values
     * @name TagsValuesUpdate
     * @request PUT:/api/tags/Values
     */
    tagsValuesUpdate: (data: ValueRequest, params: RequestParams = {}) =>
      this.request<TagHistory, any>({
        path: `/api/tags/Values`,
        method: "PUT",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
}
