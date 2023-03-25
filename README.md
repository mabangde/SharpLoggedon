## PsLoggedon 实现
该方法普通用户即可获取远程登录过用户

### 具体步骤：
依赖远程服务：RemoteRegistry


### 1. 使用RegConnectRegistryA 连接远程注册表

在另一台计算机上建立与预定义注册表项的连接。
```
LSTATUS RegConnectRegistryA(
  [in, optional] LPCSTR lpMachineName,
  [in]           HKEY   hKey,
  [out]          PHKEY  phkResult
);
```

### 2. 枚举hku(HKEY_USERS) 子键值下面sid
3. 使用ConvertStringSidToSidW
ConvertStringSidToSid 函数将字符串格式的安全标识符 (SID) 转换为有效的功能 SID。 可以使用此函数检索 ConvertSidToStringSid 函数转换为字符串格式的 SID。
```
BOOL ConvertStringSidToSidA(
  [in]  LPCSTR StringSid,
  [out] PSID   *Sid
);
```
### 4. 使用LookupAccountSidA将注册表中枚举的SID转换成用户名
LookupAccountSid 函数接受 (SID) 作为输入的安全标识符。 它检索此 SID 的帐户的名称，以及找到此 SID 的第一个域的名称。
```
BOOL LookupAccountSidA(
  [in, optional]  LPCSTR        lpSystemName,
  [in]            PSID          Sid,
  [out, optional] LPSTR         Name,
  [in, out]       LPDWORD       cchName,
  [out, optional] LPSTR         ReferencedDomainName,
  [in, out]       LPDWORD       cchReferencedDomainName,
  [out]           PSID_NAME_USE peUse
);
```
### 5. 使用RegQueryInfoKey 获取注册表信息
```
LSTATUS RegQueryInfoKey(
  HKEY hKey,   
  LPWSTR lpClass,
  LPDWORD lpcchClass,
  LPDWORD lpReserved,
  LPDWORD lpcSubKeys,
  LPDWORD lpcMaxSubKeyLen,
  LPDWORD lpcMaxClassLen,
  LPDWORD lpcValues,
  LPDWORD lpcMaxValueNameLen,
  LPDWORD lpcMaxValueLen,
  LPDWORD lpSecurityDescriptor,
  PFILETIME lpftLastWriteTime
);
```




使用bflat 编译全平台可用版本：
 `bflat build SharpLoggedon.cs`

[1] https://github.com/bflattened/bflat



